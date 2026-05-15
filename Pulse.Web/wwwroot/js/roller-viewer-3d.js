// Pulse.Web/wwwroot/js/roller-viewer-3d.js
window.RollerViewer3D = {
    scene: null,
    camera: null,
    renderer: null,
    controls: null,
    meshes: [],
    lights: [],
    isReady: false,
    animateRoller: false,
    explodeFactor: 0,
    isExploded: false,
    spinSpeed: 0.004,
    // Add these properties with other state variables
    dimensionLines: [],
    raycaster: null,
    assemblyGroup: null,
    mouse: null,
    highlightedMesh: null,
    tooltipElement: null,
    // Add to main object properties
    infoPanel: null,
    currentSelectedMesh: null,

    // ====================== FLOATING INFO PANEL & CONTEXT MENU ======================
    enableAdvancedInteractivity: function () {
        this.enableInteractivity(); // previous hover functionality

        const canvas = this.renderer.domElement;
        canvas.addEventListener('contextmenu', (e) => this._onRightClick(e));
    },

    _onRightClick: function (event) {
        event.preventDefault();

        const rect = this.renderer.domElement.getBoundingClientRect();
        this.mouse.x = ((event.clientX - rect.left) / rect.width) * 2 - 1;
        this.mouse.y = -((event.clientY - rect.top) / rect.height) * 2 + 1;

        this.raycaster.setFromCamera(this.mouse, this.camera);
        const intersects = this.raycaster.intersectObjects(this.meshes, true);

        if (intersects.length > 0) {
            let mesh = intersects[0].object;
            if (mesh.parent?.name === 'RubberCover') mesh = mesh.parent;

            this.currentSelectedMesh = mesh;
            this._showInfoPanel(mesh, event.clientX, event.clientY);
        }
    },

    _showInfoPanel: function (mesh, screenX, screenY) {
        if (!this.infoPanel) {
            this.infoPanel = document.createElement('div');
            this.infoPanel.style.position = 'absolute';
            this.infoPanel.style.background = 'rgba(30, 30, 30, 0.95)';
            this.infoPanel.style.color = '#fff';
            this.infoPanel.style.padding = '14px';
            this.infoPanel.style.borderRadius = '8px';
            this.infoPanel.style.boxShadow = '0 4px 20px rgba(0,0,0,0.6)';
            this.infoPanel.style.zIndex = '10000';
            this.infoPanel.style.minWidth = '260px';
            this.infoPanel.style.fontFamily = 'Segoe UI, sans-serif';
            this.infoPanel.style.fontSize = '14px';
            document.body.appendChild(this.infoPanel);
        }

        let title = "Unknown Part";
        let details = "";

        if (mesh.name === 'Roller') {
            title = "Roller Shell";
            details = `Material: Steel<br>Diameter: ${(mesh.geometry.parameters.radiusTop * 2).toFixed(1)} mm<br>Length: ${mesh.geometry.parameters.height.toFixed(1)} mm`;
        }
        else if (mesh.name === 'RubberCover') {
            title = "Rubber Cover";
            const outerR = mesh.userData.coverRadiusOuter || mesh.children[0]?.geometry?.parameters?.radiusTop || 0;
            details = `Type: Rubber Lining<br>Thickness: ${(outerR - (outerR - 15)).toFixed(1)} mm<br>Color: Dark`;
        }
        else if (mesh.userData.isShaft) {
            title = "Shaft";
            details = `Type: ${mesh.name}<br>Diameter: ${(mesh.geometry.parameters.radiusTop * 2).toFixed(1)} mm<br>Length: ${mesh.geometry.parameters.height.toFixed(1)} mm`;
        }

        this.infoPanel.innerHTML = `
            <strong style="font-size:16px; color:#00ddff;">${title}</strong><br><br>
            ${details}<br><br>
            <small style="color:#aaa;">Right-click to close</small>
        `;

        // Position near cursor
        let left = screenX + 20;
        let top = screenY - 10;

        // Keep panel inside viewport
        if (left + 280 > window.innerWidth) left = screenX - 280;
        if (top + 180 > window.innerHeight) top = screenY - 200;

        this.infoPanel.style.left = left + 'px';
        this.infoPanel.style.top = top + 'px';
        this.infoPanel.style.display = 'block';

        // Auto-hide when clicking elsewhere
        setTimeout(() => {
            document.addEventListener('click', this._hideInfoPanelOnce, { once: true });
        }, 100);
    },

    _hideInfoPanelOnce: function () {
        if (window.RollerViewer3D?.infoPanel) {
            window.RollerViewer3D.infoPanel.style.display = 'none';
        }
    },

    hideInfoPanel: function () {
        if (this.infoPanel) this.infoPanel.style.display = 'none';
    },

    // ====================== DIMENSION LINES & INTERACTIVITY ======================
    addDimensionLines: function () {
        this.removeDimensionLines();

        const roller = this.meshes.find(m => m.name === 'Roller') ||
            (this.assemblyGroup ? this.assemblyGroup.children.find(m => m.name === 'Roller') : null);

        if (!roller) return false;

        const length = roller.geometry.parameters.height;
        const radius = roller.geometry.parameters.radiusTop;
        const liftHeight = 90;

        // Create dimension group
        const dimGroup = new THREE.Group();
        dimGroup.name = 'dimension_lines';

        // Length dimension (along Z)
        this._createDimensionLine(
            new THREE.Vector3(-radius - 60, liftHeight + 80, -length / 2),
            new THREE.Vector3(-radius - 60, liftHeight + 80, length / 2),
            `${length.toFixed(0)} mm`,
            dimGroup
        );

        // Diameter dimension
        this._createDimensionLine(
            new THREE.Vector3(radius + 40, liftHeight - 20, 0),
            new THREE.Vector3(radius + 40, liftHeight + 20 + radius * 2, 0),
            `Ø ${(radius * 2).toFixed(0)} mm`,
            dimGroup
        );

        // Add dimension group to assembly (so it rotates with everything)
        if (this.assemblyGroup) {
            this.assemblyGroup.add(dimGroup);
        } else {
            this.scene.add(dimGroup);
        }

        this.dimensionLines.push(dimGroup);
        console.log('📐 Dimension lines added to assembly');
        return true;
    },

    _createDimensionLine: function (start, end, text, parentGroup) {
        const material = new THREE.LineBasicMaterial({ color: 0xFFFF00 });
        const points = [start, end];
        const geometry = new THREE.BufferGeometry().setFromPoints(points);
        const line = new THREE.Line(geometry, material);
        parentGroup.add(line);

        // Arrow heads
        const arrowHead1 = this._createArrowHead(start, end);
        const arrowHead2 = this._createArrowHead(end, start);
        parentGroup.add(arrowHead1, arrowHead2);

        // Text label
        const mid = new THREE.Vector3().lerpVectors(start, end, 0.5);
        mid.y += 25;
        const label = this._createTextSprite(text, 0xFFFF00);
        label.position.copy(mid);
        label.scale.set(80, 20, 1);
        parentGroup.add(label);
    },

    

    _createArrowHead: function (from, to) {
        const dir = new THREE.Vector3().subVectors(to, from).normalize();
        const arrow = new THREE.Mesh(
            new THREE.ConeGeometry(8, 20, 8),
            new THREE.MeshBasicMaterial({ color: 0xFFFF00 })
        );
        arrow.position.copy(from);
        arrow.lookAt(to);
        arrow.rotateX(Math.PI / 2);
        return arrow;
    },
    removeDimensionLines: function () {
        this.dimensionLines.forEach(obj => {
            if (obj.parent) obj.parent.remove(obj);
            else this.scene.remove(obj);
        });
        this.dimensionLines = [];
    },
    // Highlighting & Tooltips
    enableInteractivity: function () {
        this.raycaster = new THREE.Raycaster();
        this.mouse = new THREE.Vector2();

        const canvas = this.renderer.domElement;
        canvas.addEventListener('mousemove', (e) => this._onMouseMove(e));
        canvas.addEventListener('click', (e) => this._onMouseClick(e));

        // Create tooltip element
        this.tooltipElement = document.createElement('div');
        this.tooltipElement.style.position = 'absolute';
        this.tooltipElement.style.background = 'rgba(0,0,0,0.85)';
        this.tooltipElement.style.color = 'white';
        this.tooltipElement.style.padding = '8px 12px';
        this.tooltipElement.style.borderRadius = '4px';
        this.tooltipElement.style.fontSize = '13px';
        this.tooltipElement.style.pointerEvents = 'none';
        this.tooltipElement.style.zIndex = '1000';
        this.tooltipElement.style.display = 'none';
        document.body.appendChild(this.tooltipElement);
    },

    _onMouseMove: function (event) {
        const rect = this.renderer.domElement.getBoundingClientRect();
        this.mouse.x = ((event.clientX - rect.left) / rect.width) * 2 - 1;
        this.mouse.y = -((event.clientY - rect.top) / rect.height) * 2 + 1;

        this.raycaster.setFromCamera(this.mouse, this.camera);
        const intersects = this.raycaster.intersectObjects(this.meshes, true);

        if (intersects.length > 0) {
            const obj = intersects[0].object;
            const mesh = obj.parent?.name === 'RubberCover' ? obj.parent : obj;

            if (this.highlightedMesh && this.highlightedMesh !== mesh) {
                this._resetHighlight();
            }

            if (mesh && mesh.material && !mesh.name.startsWith('label_') && !mesh.name.startsWith('dimension')) {
                this.highlightedMesh = mesh;
                this._highlightMesh(mesh);
                this._showTooltip(event, mesh);
            }
        } else {
            this._resetHighlight();
            this.tooltipElement.style.display = 'none';
        }
    },

    _highlightMesh: function (mesh) {
        if (mesh.userData.originalEmissive === undefined) {
            mesh.userData.originalEmissive = mesh.material.emissive ? mesh.material.emissive.clone() : new THREE.Color(0x000000);
        }
        if (mesh.material.emissive) mesh.material.emissive.set(0x00ff88);
        if (mesh.material) mesh.material.needsUpdate = true;
    },

    _resetHighlight: function () {
        if (this.highlightedMesh) {
            if (this.highlightedMesh.material.emissive)
                this.highlightedMesh.material.emissive.copy(this.highlightedMesh.userData.originalEmissive || new THREE.Color(0x000000));
            this.highlightedMesh = null;
        }
    },

    _showTooltip: function (event, mesh) {
        let info = mesh.name;
        if (mesh.userData.isShaft) info = "Shaft";
        else if (mesh.name === 'Roller') info = "Roller Shell";
        else if (mesh.name === 'RubberCover') info = "Rubber Cover";

        this.tooltipElement.innerHTML = `<strong>${info}</strong><br>Click for details`;
        this.tooltipElement.style.left = (event.clientX + 15) + 'px';
        this.tooltipElement.style.top = (event.clientY + 15) + 'px';
        this.tooltipElement.style.display = 'block';
    },

    _onMouseClick: function (event) {
        // You can expand this later to show detailed panel
        console.log('🖱️ Clicked on 3D object');
    },

    // ===================================================================
    // INITIALIZATION
    // ===================================================================
    init: function (containerElement, width = null, height = null) {
        try {
            if (!containerElement) {
                console.error('❌ Container element is required');
                return false;
            }

            width = width || containerElement.clientWidth || 800;
            height = height || containerElement.clientHeight || 600;

            if (typeof THREE === 'undefined') {
                console.error('❌ THREE.js not loaded');
                return false;
            }

            this._doInit(containerElement, width, height);
            return true;
        } catch (error) {
            console.error('❌ Failed to initialize RollerViewer3D:', error);
            return false;
        }
    },

    _doInit: function (containerElement, width, height) {
        console.log(`🔷 Initializing RollerViewer3D (${width}×${height})`);

        // Scene
        this.scene = new THREE.Scene();
        this.scene.background = new THREE.Color(0xf0f4f8);
        this.spinSpeed = 0.004;
        // Camera
        this.camera = new THREE.PerspectiveCamera(60, width / height, 0.1, 10000);
        this.camera.position.set(400, 300, 500);

        // Renderer
        this.renderer = new THREE.WebGLRenderer({
            antialias: true,
            alpha: true,
            preserveDrawingBuffer: true
        });
        this.renderer.setSize(width, height);
        this.renderer.setPixelRatio(Math.min(window.devicePixelRatio, 2));
        this.renderer.shadowMap.enabled = true;
        this.renderer.shadowMap.type = THREE.PCFShadowMap;
        this.renderer.outputColorSpace = THREE.SRGBColorSpace;
        this.renderer.toneMapping = THREE.ACESFilmicToneMapping;
        this.renderer.toneMappingExposure = 1.1;

        containerElement.appendChild(this.renderer.domElement);
        const floorGeometry = new THREE.PlaneGeometry(10000, 10000);
        const floorMaterial = new THREE.MeshStandardMaterial({
            color: 0xf0f2f5,
            roughness: 0.8,
            metalness: 0.02,
        });

        const floor = new THREE.Mesh(floorGeometry, floorMaterial);
        floor.receiveShadow = true;
        floor.castShadow = false;
        floor.rotation.x = -Math.PI / 2;
        floor.position.y = -200;     // Keep this or adjust if needed
        this.scene.add(floor);

        // Subtle grid helper
        const grid = new THREE.GridHelper(10000, 10000, 0xcccccc, 0xdddddd);
        grid.position.y = -199.5;
        grid.material.opacity = 0.5;
        grid.material.transparent = true;
        this.scene.add(grid);
        this._setupLightsAndEnvironment();
        this._initializeOrbitControls();
        this._setupResizeHandler(containerElement);

        this.animate();
        this.isReady = true;

        console.log('✅ RollerViewer3D initialized successfully');
        return true;
    },

    getPartsList: function () {
        const parts = [];

        const traverse = (obj) => {
            if (obj.userData && obj.userData.isShaft) {
                parts.push({
                    name: obj.name,
                    displayName: this._getDisplayName(obj.name),
                    visible: obj.visible !== false,
                    opacity: obj.material ? (obj.material.opacity || 1.0) : 1.0
                });
            } else if (obj.name === 'Roller' || obj.name === 'RubberCover') {
                parts.push({
                    name: obj.name,
                    displayName: this._getDisplayName(obj.name),
                    visible: obj.visible !== false,
                    opacity: obj.material ? (obj.material.opacity || 1.0) : 1.0
                });
            }

            if (obj.children) {
                obj.children.forEach(child => traverse(child));
            }
        };

        if (this.assemblyGroup) {
            traverse(this.assemblyGroup);
        } else {
            this.meshes.forEach(mesh => traverse(mesh));
        }

        return parts;
    },
        
    // getPartsList: function () {
    //     return this.meshes
    //         .filter(mesh => mesh.name && !mesh.name.startsWith('label_') && !mesh.name.startsWith('dimension'))
    //         .map(mesh => ({
    //             name: mesh.name,
    //             displayName: this._getDisplayName(mesh.name),
    //             visible: mesh.visible !== true
    //         }));
    // },

    _getDisplayName: function (name) {
        if (name === 'Roller') return 'Roller Shell';
        if (name === 'RubberCover') return 'Rubber Cover';
        if (name.startsWith('shaft_')) return 'Shaft ' + name.replace('shaft_', '');
        return name;
    },

    toggleVisibility: function (meshName, visible) {
        let found = false;

        const traverse = (obj) => {
            if (obj.name === meshName) {
                obj.visible = visible;
                found = true;
            }
            if (obj.children) {
                obj.children.forEach(child => traverse(child));
            }
        };

        if (this.assemblyGroup) {
            traverse(this.assemblyGroup);
        } else {
            this.meshes.forEach(mesh => traverse(mesh));
        }

        return found;
    },
    setAllVisibility: function (visible) {
        this.meshes.forEach(mesh => {
            if (mesh.name && !mesh.name.startsWith('label_') && !mesh.name.startsWith('dimension')) {
                mesh.visible = visible;
            }
        });
        console.log(`👁️ All parts visibility set to ${visible}`);
    },

    setOpacity: function (meshName, opacity) {
        let found = false;

        const applyOpacityToObject = (obj, targetOpacity) => {
            if (obj.material) {
                if (Array.isArray(obj.material)) {
                    obj.material.forEach(mat => {
                        mat.transparent = true;
                        mat.opacity = Math.max(0.1, Math.min(1.0, targetOpacity));
                        mat.needsUpdate = true;
                    });
                } else {
                    obj.material.transparent = true;
                    obj.material.opacity = Math.max(0.1, Math.min(1.0, targetOpacity));
                    obj.material.needsUpdate = true;
                }
            }
        };

        const traverse = (obj) => {
            if (obj.name === meshName) {
                // Apply to the object itself
                applyOpacityToObject(obj, opacity);
                found = true;

                // If it's a group (like RubberCover), apply to all children too
                if (obj.children && obj.children.length > 0) {
                    obj.children.forEach(child => {
                        applyOpacityToObject(child, opacity);
                    });
                }
            }

            if (obj.children) {
                obj.children.forEach(child => traverse(child));
            }
        };

        if (this.assemblyGroup) {
            traverse(this.assemblyGroup);
        } else {
            this.meshes.forEach(mesh => traverse(mesh));
        }

        console.log(`🎨 Opacity set for ${meshName}: ${opacity}`);
        return found;
    },

    _applyOpacity: function (material, opacity) {
        material.transparent = true;
        material.opacity = Math.max(0.1, Math.min(1.0, opacity));
        material.needsUpdate = true;
    },

    // Reset all opacities to 1.0
    resetAllOpacities: function () {
        const resetOpacity = (obj) => {
            if (obj.material) {
                if (Array.isArray(obj.material)) {
                    obj.material.forEach(mat => {
                        mat.transparent = false;
                        mat.opacity = 1.0;
                    });
                } else {
                    obj.material.transparent = false;
                    obj.material.opacity = 1.0;
                }
            }
        };

        const traverse = (obj) => {
            resetOpacity(obj);
            if (obj.children) {
                obj.children.forEach(child => traverse(child));
            }
        };

        if (this.assemblyGroup) {
            traverse(this.assemblyGroup);
        } else {
            this.meshes.forEach(mesh => traverse(mesh));
        }

        console.log('🔄 All opacities reset');
    },

    _setupLightsAndEnvironment: function () {
        // Clear existing lights
        this.lights.forEach(light => this.scene.remove(light));
        this.lights = [];

        // === Simple but Good Looking Environment (No external loader needed) ===
        this.scene.background = new THREE.Color(0xf0f4f8);

        // Environment map using CubeTexture (much more reliable)
        const envMap = new THREE.CubeTextureLoader().load([
            'https://raw.githubusercontent.com/mrdoob/three.js/dev/examples/textures/cube/Bridge2/posx.jpg',
            'https://raw.githubusercontent.com/mrdoob/three.js/dev/examples/textures/cube/Bridge2/negx.jpg',
            'https://raw.githubusercontent.com/mrdoob/three.js/dev/examples/textures/cube/Bridge2/posy.jpg',
            'https://raw.githubusercontent.com/mrdoob/three.js/dev/examples/textures/cube/Bridge2/negy.jpg',
            'https://raw.githubusercontent.com/mrdoob/three.js/dev/examples/textures/cube/Bridge2/posz.jpg',
            'https://raw.githubusercontent.com/mrdoob/three.js/dev/examples/textures/cube/Bridge2/negz.jpg'
        ]);

        this.scene.environment = envMap;

        console.log('✅ Cube Environment Map loaded');

        // === Improved Lighting Setup ===
        const ambient = new THREE.AmbientLight(0xffffff, 0.85);
        this.scene.add(ambient);

        const hemi = new THREE.HemisphereLight(0xe0f0ff, 0x808080, 0.9);
        this.scene.add(hemi);

        // Key Light
        const keyLight = new THREE.DirectionalLight(0xffffff, 1.5);
        keyLight.position.set(300, 500, 400);
        keyLight.castShadow = true;
        
        this.scene.add(keyLight);

        // Fill Light
        const fillLight = new THREE.DirectionalLight(0xcceeff, 0.9);
        fillLight.position.set(-300, 350, -300);
        this.scene.add(fillLight);

        // Rim Light
        const rimLight = new THREE.DirectionalLight(0xffffff, 0.7);
        rimLight.position.set(0, 200, -500);
        this.scene.add(rimLight);

        console.log('✅ Enhanced lighting with environment reflections active');
    },

    _initializeOrbitControls: function () {
        if (typeof OrbitControls === 'undefined') {
            console.warn('⚠️ OrbitControls not available yet');
            setTimeout(() => this._initializeOrbitControls(), 150);
            return;
        }

        this.controls = new OrbitControls(this.camera, this.renderer.domElement);
        this.controls.enableDamping = true;
        this.controls.dampingFactor = 0.08;
        this.controls.enableZoom = true;
        this.controls.minDistance = 100;
        this.controls.maxDistance = 5000;
        this.controls.target.set(0, 80, 0);
    },

    _setupResizeHandler: function (container) {
        const handler = () => {
            if (!this.camera || !this.renderer) return;
            const w = container.clientWidth;
            const h = container.clientHeight;
            if (w && h) {
                this.camera.aspect = w / h;
                this.camera.updateProjectionMatrix();
                this.renderer.setSize(w, h);
            }
        };
        window.addEventListener('resize', handler);
    },

    // ===================================================================
    // MODEL BUILDING METHODS (Call from Blazor)
    // ===================================================================
    clearScene: function () {
        this.meshes.forEach(mesh => this.scene.remove(mesh));
        this.meshes = [];
        this.animateRoller = false;
        console.log('🧹 Scene cleared');
    },

    addRoller: function (diameter, length) {
        const radius = diameter / 2;

        const geometry = new THREE.CylinderGeometry(radius, radius, length, 64, 8);
        const material = new THREE.MeshStandardMaterial({
            color: 0xa8b5c0,
            metalness: 0.88,
            roughness: 0.15,
            envMapIntensity: 1.4
        });

        const roller = new THREE.Mesh(geometry, material);
        roller.name = 'Roller';
        roller.castShadow = true;
        roller.receiveShadow = true;
        

        this.scene.add(roller);
        this.meshes.push(roller);
        console.log(`✅ Roller added (Ø${diameter} × ${length}mm)`);
        return true;
    },

    addRubberCover: function (leftOffset, coverLength, coverThickness, color = 0x1a1a1a) {
        const roller = this.meshes.find(m => m.name === 'Roller');
        if (!roller) return false;

        const rollerRadius = roller.geometry.parameters.radiusTop;
        const innerR = rollerRadius;
        const outerR = rollerRadius + coverThickness;

        const group = new THREE.Group();
        group.name = 'RubberCover';

        const mat = new THREE.MeshStandardMaterial({
            color: color || 0x1a1a1a,
            metalness: 0.1,
            roughness: 0.82,
            envMapIntensity: 0.4
        });

        // Outer
        const outer = new THREE.Mesh(
            new THREE.CylinderGeometry(outerR, outerR, coverLength, 64, 1, true),
            mat
        );
        // Inner
        const inner = new THREE.Mesh(
            new THREE.CylinderGeometry(innerR, innerR, coverLength, 64, 1, true),
            mat
        );

        // End rings (thickness)
        const ringGeo = new THREE.RingGeometry(innerR, outerR, 64);
        const topRing = new THREE.Mesh(ringGeo, mat);
        topRing.rotation.x = -Math.PI / 2;
        topRing.position.y = coverLength / 2;

        const bottomRing = topRing.clone();
        bottomRing.rotation.x = Math.PI / 2;
        bottomRing.position.y = -coverLength / 2;

        group.add(outer, inner, topRing, bottomRing);
        group.position.y = leftOffset;           // axial offset
        group.userData.coverRadiusOuter = outerR;
        

        this.scene.add(group);
        this.meshes.push(group);

        console.log(`✅ Rubber cover added (thickness ${coverThickness}mm)`);
        return true;
    },

    addShaft: function (outerDiameter, length, axialPosition, side = "center", radialOffset = 0, name = "") {
        const radius = outerDiameter / 2;
        const shaftName = name ? `shaft_${name}` : `shaft_${this.meshes.length}`;

        // const material = new THREE.MeshStandardMaterial({
        //     color: 0x444444,
        //     metalness: 0.92,
        //     roughness: 0.18
        // });

        const material = new THREE.MeshStandardMaterial({
            color: 0xa8b5c0,
            metalness: 0.88,
            roughness: 0.15,
            envMapIntensity: 1.4
        });
        const shaftMesh = new THREE.Mesh(
            new THREE.CylinderGeometry(radius, radius, length, 48),
            material
        );

        shaftMesh.name = shaftName;
        shaftMesh.castShadow = true;
        shaftMesh.receiveShadow = true;

        // Store positioning data directly on the mesh
        shaftMesh.userData.axialPos = axialPosition;
        shaftMesh.userData.radialOffset = radialOffset;
        shaftMesh.userData.side = side.toLowerCase();
        shaftMesh.userData.isShaft = true;

        this.scene.add(shaftMesh);
        this.meshes.push(shaftMesh);

        console.log(`✅ Simple shaft added: ${shaftName} | Length: ${length}mm | Axial: ${axialPosition.toFixed(2)}mm`);
        return true;
    },

    // ===================================================================
    // FINAL ASSEMBLY & POSITIONING (Most Important!)
    // ===================================================================
    applyFinalPositioning: function () {
        if (this.meshes.length === 0) return false;

        const liftHeight = 90;

        // Create main assembly group
        this.assemblyGroup = new THREE.Group();
        this.assemblyGroup.name = "RollerAssembly";
        this.scene.add(this.assemblyGroup);

        this.meshes.forEach(mesh => {
            // Rotate to horizontal
            mesh.rotation.x = Math.PI / 2;
            mesh.rotation.y = 0;
            mesh.rotation.z = 0;

            mesh.position.y = liftHeight;

            if (mesh.userData.isShaft) {
                mesh.position.z = mesh.userData.axialPos || 0;
                const offset = mesh.userData.radialOffset || 0;
                mesh.position.x = (mesh.userData.side === "left") ? -offset : offset;
            } else {
                mesh.position.x = 0;
                mesh.position.z = 0;
            }

            // Add everything to the main assembly group
            this.assemblyGroup.add(mesh);
        });

        // Clear old meshes array and put only the assembly group
        this.meshes = [this.assemblyGroup];

        this._positionCamera();
        this.animateRoller = true;

        console.log('✅ All parts grouped into one rotating assembly');
        return true;
    },

    _positionCamera: function () {
        const roller = this.meshes.find(m => m.name === 'Roller');
        if (!roller) return;

        const liftHeight = 90;
        const bbox = new THREE.Box3().setFromObject(roller);
        const size = bbox.getSize(new THREE.Vector3());

        // Make camera distance larger to account for long shafts
        const distance = Math.max(size.z * 1.8, 800);

        this.camera.position.set(distance * 0.7, liftHeight + size.y * 1.2, distance * 0.9);
        this.camera.lookAt(0, liftHeight, 0);

        if (this.controls) {
            this.controls.target.set(0, liftHeight, 0);
            this.controls.update();
        }
    },
    // ===================================================================
    // ANIMATION & UTILITIES
    // ===================================================================
    animate: function () {
        requestAnimationFrame(() => this.animate());

        if (this.animateRoller && this.assemblyGroup) {
            const speed = this.spinSpeed || 0.004;
            this.assemblyGroup.rotation.y += speed;
        }

        if (this.controls) this.controls.update();
        if (this.renderer && this.scene && this.camera) {
            this.renderer.render(this.scene, this.camera);
        }
    },
    debugPositions: function () {
        console.group('🔍 RollerViewer3D - Current Positions');
        this.meshes.forEach(m => {
            console.log(`${m.name.padEnd(18)} | ` +
                `Pos: (${m.position.x.toFixed(1)}, ${m.position.y.toFixed(1)}, ${m.position.z.toFixed(1)}) | ` +
                `Rot: (${m.rotation.x.toFixed(2)}, ${m.rotation.y.toFixed(2)}, ${m.rotation.z.toFixed(2)})`);
        });
        console.groupEnd();
    },

    dispose: function () {
        this.clearScene();
        if (this.renderer) this.renderer.dispose();
        console.log('🗑️ RollerViewer3D disposed');
    },
        // ====================== ANIMATION & CAMERA CONTROLS ======================
    setSpinSpeed: function (speed) {
        // speed: 0 = stopped, 0.001 = slow, 0.008 = fast
        this.spinSpeed = Math.max(0, Math.min(speed, 0.1));
        console.log(`🔄 Spin speed set to ${this.spinSpeed}`);
    },

    startSpin: function () {
        this.animateRoller = true;
        console.log('▶️ Roller spin started');
    },

    stopSpin: function () {
        this.animateRoller = false;
        console.log('⏸️ Roller spin stopped');
    },

    toggleSpin: function () {
        this.animateRoller = !this.animateRoller;
        console.log(this.animateRoller ? '▶️ Spin started' : '⏸️ Spin stopped');
        return this.animateRoller;
    },

    resetCamera: function () {
        this._positionCamera();
        console.log('📷 Camera reset to default view');
    },

    setCameraPreset: function (preset) {
        const lift = 90;
        switch (preset.toLowerCase()) {
            case 'front':
                this.camera.position.set(0, lift + 100, 600);
                break;
            case 'side':
                this.camera.position.set(600, lift + 150, 0);
                break;
            case 'top':
                this.camera.position.set(0, lift + 800, 50);
                break;
            case 'isometric':
            default:
                this.camera.position.set(450, lift + 350, 450);
                break;
        }
        if (this.controls) {
            this.controls.target.set(0, lift, 0);
            this.controls.update();
        }
        console.log(`📷 Camera preset: ${preset}`);
    },

    zoomToFit: function () {
        this._positionCamera();
    },
    // ====================== ADVANCED FEATURES ======================
    explodeView: function (factor = 0) {
        if (!this.assemblyGroup) return;

        this.explodeFactor = Math.max(0, Math.min(factor, 1));

        this.assemblyGroup.children.forEach(child => {
            if (child.userData.isShaft) {
                const originalZ = child.userData.axialPos || 0;
                child.position.z = originalZ * (1 + this.explodeFactor);
            } else if (child.name === 'RubberCover') {
                child.position.y = 90 + (90 * this.explodeFactor) * (1 + this.explodeFactor); // 60;
            }
        });

        console.log(`💥 Exploded view factor: ${this.explodeFactor}`);
    },

    toggleExplodedView: function () {
        this.isExploded = !this.isExploded;
        this.explodeView(this.isExploded ? 1 : 0);
        return this.isExploded;
    },

    captureScreenshot: function () {
        try {
            const link = document.createElement('a');
            link.download = `Roller_${new Date().toISOString().slice(0, 19).replace(/:/g, '-')}.png`;
            link.href = this.renderer.domElement.toDataURL('image/png', 1.0);
            link.click();
            console.log('📸 Screenshot captured');
            return true;
        } catch (e) {
            console.error('❌ Screenshot failed', e);
            return false;
        }
    },

    addMeasurementLabels: function () {
        this.removeMeasurementLabels();

        const roller = this.meshes.find(m => m.name === 'Roller') ||
            (this.assemblyGroup ? this.assemblyGroup.children.find(m => m.name === 'Roller') : null);

        if (!roller) return false;

        const length = roller.geometry.parameters.height;
        const diameter = roller.geometry.parameters.radiusTop * 2;
        const liftHeight = 90;

        const labelGroup = new THREE.Group();
        labelGroup.name = 'measurement_labels';

        // Length label
        const lengthLabel = this._createTextSprite(`L = ${length.toFixed(0)} mm`, 0xFFFFFF);
        lengthLabel.position.set(0, liftHeight + 140, length / 2 + 30);
        labelGroup.add(lengthLabel);

        // Diameter label
        const diaLabel = this._createTextSprite(`Ø ${diameter.toFixed(0)} mm`, 0xFFFF00);
        diaLabel.position.set(diameter / 2 + 40, liftHeight + 100, 0);
        labelGroup.add(diaLabel);

        // Add to assembly
        if (this.assemblyGroup) {
            this.assemblyGroup.add(labelGroup);
        } else {
            this.scene.add(labelGroup);
        }

        this.dimensionLines.push(labelGroup); // Reuse dimensionLines array for cleanup
        console.log('📏 Measurement labels added to assembly');
        return true;
    },

    _createTextSprite: function (message, color = 0xFFFFFF) {
        const canvas = document.createElement('canvas');
        const ctx = canvas.getContext('2d');
        canvas.width = 512;
        canvas.height = 128;

        ctx.fillStyle = 'rgba(0,0,0,0.7)';
        ctx.fillRect(0, 0, canvas.width, canvas.height);
        ctx.font = 'bold 48px Arial';
        ctx.fillStyle = '#' + color.toString(16).padStart(6, '0');
        ctx.textAlign = 'center';
        ctx.textBaseline = 'middle';
        ctx.fillText(message, canvas.width / 2, canvas.height / 2);

        const texture = new THREE.CanvasTexture(canvas);
        const material = new THREE.SpriteMaterial({ map: texture });
        const sprite = new THREE.Sprite(material);
        sprite.scale.set(120, 30, 1);
        return sprite;
    },

    removeMeasurementLabels: function () {
        this.dimensionLines.forEach(obj => {
            if (obj.parent) obj.parent.remove(obj);
            else this.scene.remove(obj);
        });
        this.dimensionLines = [];
    }
};