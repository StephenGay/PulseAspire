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
    dimensionLines: [],
    raycaster: null,
    assemblyGroup: null,
    mouse: null,
    composer: null,
    renderPass: null,
    bloomPass: null,
    fxaaPass: null,
    usePostProcessing: true,
    highlightedMesh: null,
    tooltipElement: null,
    infoPanel: null,
    currentSelectedMesh: null,

    // ====================== INITIALIZATION ======================
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

        this.scene = new THREE.Scene();
        this.scene.background = new THREE.Color(0x0f172a);
        this.spinSpeed = 0.004;

        this.camera = new THREE.PerspectiveCamera(60, width / height, 0.1, 10000);
        this.camera.position.set(400, 300, 500);

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

        // Floor
        const floorGeometry = new THREE.PlaneGeometry(10000, 10000);
        const floorMaterial = new THREE.MeshStandardMaterial({
            color: 0x1e2937,           // Dark slate gray
            roughness: 0.55,
            metalness: 0.08,
        });
        const floor = new THREE.Mesh(floorGeometry, floorMaterial);
        floor.receiveShadow = true;
        floor.castShadow = false;
        floor.rotation.x = -Math.PI / 2;
        floor.position.y = -200;
        this.scene.add(floor);

        // Grid
        const grid = new THREE.GridHelper(10000, 50, 0x64748b, 0x334155);
        grid.position.y = -199.5;
        grid.material.opacity = 0.55;
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

    // ====================== POST-PROCESSING ======================
    initPostProcessing: function () {
        if (!this.renderer || !this.scene || !this.camera) {
            console.error('❌ Cannot initialize post-processing: renderer/scene/camera not ready');
            return false;
        }
        try {
            this.composer = new THREE.EffectComposer(this.renderer);
            this.renderPass = new THREE.RenderPass(this.scene, this.camera);
            this.composer.addPass(this.renderPass);

            this.bloomPass = new THREE.UnrealBloomPass(
                new THREE.Vector2(window.innerWidth, window.innerHeight),
                1.2, 0.8, 0.85
            );
            this.composer.addPass(this.bloomPass);

            this.fxaaPass = new THREE.ShaderPass(THREE.FXAAShader);
            this.fxaaPass.uniforms['resolution'].value.set(1 / window.innerWidth, 1 / window.innerHeight);
            this.composer.addPass(this.fxaaPass);

            this.renderer.toneMapping = THREE.ACESFilmicToneMapping;
            this.renderer.toneMappingExposure = 1.3;

            console.log('✅ Post-processing initialized (Bloom + FXAA + ACES)');
            return true;
        } catch (error) {
            console.error('❌ Failed to initialize post-processing:', error);
            return false;
        }
    },

    disablePostProcessing: function () {
        this.usePostProcessing = false;
        if (this.renderer) {
            this.renderer.toneMapping = THREE.ACESFilmicToneMapping;
            this.renderer.toneMappingExposure = 1.1;
        }
        console.log('🔄 Post-processing disabled');
    },

    // ====================== ANIMATION ======================
    animate: function () {
        requestAnimationFrame(() => this.animate());

        if (this.animateRoller && this.assemblyGroup) {
            const speed = this.spinSpeed || 0.004;
            this.assemblyGroup.rotation.y += speed;
        }

        if (this.controls) this.controls.update();

        if (this.usePostProcessing && this.composer) {
            this.composer.render();
        } else if (this.renderer && this.scene && this.camera) {
            this.renderer.render(this.scene, this.camera);
        }
    },

    // ====================== MODEL BUILDING ======================
    clearScene: function () {
        this.meshes.forEach(mesh => this.scene.remove(mesh));
        this.meshes = [];
        this.animateRoller = false;
        this.assemblyGroup = null;
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

        const outer = new THREE.Mesh(new THREE.CylinderGeometry(outerR, outerR, coverLength, 64, 1, true), mat);
        const inner = new THREE.Mesh(new THREE.CylinderGeometry(innerR, innerR, coverLength, 64, 1, true), mat);

        const ringGeo = new THREE.RingGeometry(innerR, outerR, 64);
        const topRing = new THREE.Mesh(ringGeo, mat);
        topRing.rotation.x = -Math.PI / 2;
        topRing.position.y = coverLength / 2;

        const bottomRing = topRing.clone();
        bottomRing.rotation.x = Math.PI / 2;
        bottomRing.position.y = -coverLength / 2;

        group.add(outer, inner, topRing, bottomRing);
        group.position.y = leftOffset;
        group.userData.coverRadiusOuter = outerR;

        this.scene.add(group);
        this.meshes.push(group);
        console.log(`✅ Rubber cover added (thickness ${coverThickness}mm)`);
        return true;
    },

    addShaft: function (outerDiameter, length, axialPosition, side = "center", radialOffset = 0, name = "") {
        const radius = outerDiameter / 2;
        const shaftName = name ? `shaft_${name}` : `shaft_${this.meshes.length}`;

        const material = new THREE.MeshStandardMaterial({
            color: 0x444444,
            metalness: 0.92,
            roughness: 0.15
        });

        const shaftMesh = new THREE.Mesh(
            new THREE.CylinderGeometry(radius, radius, length, 48),
            material
        );

        shaftMesh.name = shaftName;
        shaftMesh.castShadow = true;
        shaftMesh.receiveShadow = true;

        shaftMesh.userData.axialPos = axialPosition;
        shaftMesh.userData.radialOffset = radialOffset;
        shaftMesh.userData.side = side.toLowerCase();
        shaftMesh.userData.isShaft = true;

        this.scene.add(shaftMesh);
        this.meshes.push(shaftMesh);

        console.log(`✅ Shaft added: ${shaftName} | Length: ${length}mm | Axial: ${axialPosition.toFixed(2)}mm`);
        return true;
    },

    // ====================== FINAL POSITIONING ======================
    applyFinalPositioning: function () {
        if (this.meshes.length === 0) return false;

        const liftHeight = 90;

        this.assemblyGroup = new THREE.Group();
        this.assemblyGroup.name = "RollerAssembly";
        this.scene.add(this.assemblyGroup);

        this.meshes.forEach(mesh => {
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

            this.assemblyGroup.add(mesh);
        });

        this.meshes = [this.assemblyGroup];
        this._positionCamera();
        this.animateRoller = true;

        console.log('✅ All parts grouped into one rotating assembly');
        return true;
    },

    _positionCamera: function () {
        const roller = this.assemblyGroup ? this.assemblyGroup.children.find(m => m.name === 'Roller') : null;
        if (!roller) return;

        const liftHeight = 90;
        const bbox = new THREE.Box3().setFromObject(roller);
        const size = bbox.getSize(new THREE.Vector3());
        const distance = Math.max(size.z * 1.8, 800);

        this.camera.position.set(distance * 0.7, liftHeight + size.y * 1.2, distance * 0.9);
        this.camera.lookAt(0, liftHeight, 0);

        if (this.controls) {
            this.controls.target.set(0, liftHeight, 0);
            this.controls.update();
        }
    },

    // ====================== ANIMATION CONTROLS ======================
    setSpinSpeed: function (speed) {
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
            case 'front': this.camera.position.set(0, lift + 100, 600); break;
            case 'side': this.camera.position.set(600, lift + 150, 0); break;
            case 'top': this.camera.position.set(0, lift + 800, 50); break;
            default: this.camera.position.set(450, lift + 350, 450); break;
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

    // ====================== EXPLODED VIEW ======================
    explodeView: function (factor = 0) {
        if (!this.assemblyGroup) return;
        this.explodeFactor = Math.max(0, Math.min(factor, 1));

        this.assemblyGroup.children.forEach(child => {
            if (child.userData.isShaft) {
                const originalZ = child.userData.axialPos || 0;
                child.position.z = originalZ * (1 + this.explodeFactor);
            } else if (child.name === 'RubberCover') {
                child.position.y = 90 + (90 * this.explodeFactor) * (1 + this.explodeFactor);
            }
        });
        console.log(`💥 Exploded view factor: ${this.explodeFactor}`);
    },

    toggleExplodedView: function () {
        this.isExploded = !this.isExploded;
        this.explodeView(this.isExploded ? 1 : 0);
        return this.isExploded;
    },

    // ====================== PARTS MANAGEMENT ======================
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
            if (obj.children) obj.children.forEach(child => traverse(child));
        };

        if (this.assemblyGroup) traverse(this.assemblyGroup);
        else this.meshes.forEach(mesh => traverse(mesh));
        return parts;
    },

    _getDisplayName: function (name) {
        if (name === 'Roller') return 'Roller Shell';
        if (name === 'RubberCover') return 'Rubber Cover';
        if (name.startsWith('shaft_')) return 'Shaft ' + name.replace('shaft_', '');
        return name;
    },

    toggleVisibility: function (meshName, visible) {
        let found = false;
        const traverse = (obj) => {
            if (obj.name === meshName) { obj.visible = visible; found = true; }
            if (obj.children) obj.children.forEach(child => traverse(child));
        };
        if (this.assemblyGroup) traverse(this.assemblyGroup);
        else this.meshes.forEach(mesh => traverse(mesh));
        return found;
    },

    setAllVisibility: function (visible) {
        const traverse = (obj) => {
            if (obj.name && !obj.name.startsWith('label_') && !obj.name.startsWith('dimension')) {
                obj.visible = visible;
            }
            if (obj.children) obj.children.forEach(child => traverse(child));
        };
        if (this.assemblyGroup) traverse(this.assemblyGroup);
        else this.meshes.forEach(mesh => traverse(mesh));
        console.log(`👁️ All parts visibility set to ${visible}`);
    },

    setOpacity: function (meshName, opacity) {
        let found = false;
        const applyOpacity = (obj, targetOpacity) => {
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
                applyOpacity(obj, opacity);
                found = true;
                if (obj.children && obj.children.length > 0) {
                    obj.children.forEach(child => applyOpacity(child, opacity));
                }
            }
            if (obj.children) obj.children.forEach(child => traverse(child));
        };

        if (this.assemblyGroup) traverse(this.assemblyGroup);
        else this.meshes.forEach(mesh => traverse(mesh));

        console.log(`🎨 Opacity set for ${meshName}: ${opacity}`);
        return found;
    },

    resetAllOpacities: function () {
        const reset = (obj) => {
            if (obj.material) {
                if (Array.isArray(obj.material)) {
                    obj.material.forEach(mat => { mat.transparent = false; mat.opacity = 1.0; });
                } else {
                    obj.material.transparent = false;
                    obj.material.opacity = 1.0;
                }
            }
        };
        const traverse = (obj) => { reset(obj); if (obj.children) obj.children.forEach(child => traverse(child)); };
        if (this.assemblyGroup) traverse(this.assemblyGroup);
        else this.meshes.forEach(mesh => traverse(mesh));
        console.log('🔄 All opacities reset');
    },

    // ====================== ADVANCED FEATURES ======================
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

    // ====================== DIMENSION LINES & LABELS ======================
    addDimensionLines: function () {
        this.removeDimensionLines();
        const roller = this.assemblyGroup ? this.assemblyGroup.children.find(m => m.name === 'Roller') : null;
        if (!roller) return false;

        const length = roller.geometry.parameters.height;
        const radius = roller.geometry.parameters.radiusTop;
        const liftHeight = 90;

        const dimGroup = new THREE.Group();
        dimGroup.name = 'dimension_lines';

        this._createDimensionLine(
            new THREE.Vector3(-radius - 60, liftHeight + 80, -length / 2),
            new THREE.Vector3(-radius - 60, liftHeight + 80, length / 2),
            `${length.toFixed(0)} mm`,
            dimGroup
        );

        this._createDimensionLine(
            new THREE.Vector3(radius + 40, liftHeight - 20, 0),
            new THREE.Vector3(radius + 40, liftHeight + 20 + radius * 2, 0),
            `Ø ${(radius * 2).toFixed(0)} mm`,
            dimGroup
        );

        if (this.assemblyGroup) this.assemblyGroup.add(dimGroup);
        else this.scene.add(dimGroup);

        this.dimensionLines.push(dimGroup);
        console.log('📐 Dimension lines added');
        return true;
    },

    _createDimensionLine: function (start, end, text, parentGroup) {
        const material = new THREE.LineBasicMaterial({ color: 0xFFFF00 });
        const line = new THREE.Line(new THREE.BufferGeometry().setFromPoints([start, end]), material);
        parentGroup.add(line);

        const arrow1 = this._createArrowHead(start, end);
        const arrow2 = this._createArrowHead(end, start);
        parentGroup.add(arrow1, arrow2);

        const mid = new THREE.Vector3().lerpVectors(start, end, 0.5);
        mid.y += 25;
        const label = this._createTextSprite(text, 0xFFFF00);
        label.position.copy(mid);
        label.scale.set(80, 20, 1);
        parentGroup.add(label);
    },

    _createArrowHead: function (from, to) {
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

    addMeasurementLabels: function () {
        this.removeMeasurementLabels();
        const roller = this.assemblyGroup ? this.assemblyGroup.children.find(m => m.name === 'Roller') : null;
        if (!roller) return false;

        const length = roller.geometry.parameters.height;
        const diameter = roller.geometry.parameters.radiusTop * 2;
        const liftHeight = 90;

        const labelGroup = new THREE.Group();
        labelGroup.name = 'measurement_labels';

        const lengthLabel = this._createTextSprite(`L = ${length.toFixed(0)} mm`, 0xFFFFFF);
        lengthLabel.position.set(0, liftHeight + 140, length / 2 + 30);
        labelGroup.add(lengthLabel);

        const diaLabel = this._createTextSprite(`Ø ${diameter.toFixed(0)} mm`, 0xFFFF00);
        diaLabel.position.set(diameter / 2 + 40, liftHeight + 100, 0);
        labelGroup.add(diaLabel);

        if (this.assemblyGroup) this.assemblyGroup.add(labelGroup);
        else this.scene.add(labelGroup);

        this.dimensionLines.push(labelGroup);
        console.log('📏 Measurement labels added');
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
    },

    // ====================== INTERACTIVITY ======================
    enableAdvancedInteractivity: function () {
        this.enableInteractivity();
        const canvas = this.renderer.domElement;
        canvas.addEventListener('contextmenu', (e) => this._onRightClick(e));
    },

    enableInteractivity: function () {
        this.raycaster = new THREE.Raycaster();
        this.mouse = new THREE.Vector2();
        const canvas = this.renderer.domElement;
        canvas.addEventListener('mousemove', (e) => this._onMouseMove(e));
        canvas.addEventListener('click', (e) => this._onMouseClick(e));

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
        const intersects = this.raycaster.intersectObjects(this.assemblyGroup ? [this.assemblyGroup] : this.meshes, true);

        if (intersects.length > 0) {
            const obj = intersects[0].object;
            const mesh = obj.parent?.name === 'RubberCover' ? obj.parent : obj;

            if (this.highlightedMesh && this.highlightedMesh !== mesh) this._resetHighlight();

            if (mesh && mesh.material && !mesh.name.startsWith('label_') && !mesh.name.startsWith('dimension')) {
                this.highlightedMesh = mesh;
                this._highlightMesh(mesh);
                this._showTooltip(event, mesh);
            }
        } else {
            this._resetHighlight();
            if (this.tooltipElement) this.tooltipElement.style.display = 'none';
        }
    },

    _highlightMesh: function (mesh) {
        if (mesh.userData.originalEmissive === undefined) {
            mesh.userData.originalEmissive = mesh.material.emissive ? mesh.material.emissive.clone() : new THREE.Color(0x000000);
        }
        if (mesh.material.emissive) mesh.material.emissive.set(0x00ff88);
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

        if (this.tooltipElement) {
            this.tooltipElement.innerHTML = `<strong>${info}</strong><br>Click for details`;
            this.tooltipElement.style.left = (event.clientX + 15) + 'px';
            this.tooltipElement.style.top = (event.clientY + 15) + 'px';
            this.tooltipElement.style.display = 'block';
        }
    },

    _onMouseClick: function (event) {
        console.log('🖱️ Clicked on 3D object');
    },

    // ====================== RIGHT-CLICK INFO PANEL ======================
    _onRightClick: function (event) {
        event.preventDefault();
        const rect = this.renderer.domElement.getBoundingClientRect();
        this.mouse.x = ((event.clientX - rect.left) / rect.width) * 2 - 1;
        this.mouse.y = -((event.clientY - rect.top) / rect.height) * 2 + 1;

        this.raycaster.setFromCamera(this.mouse, this.camera);
        const intersects = this.raycaster.intersectObjects(this.assemblyGroup ? [this.assemblyGroup] : this.meshes, true);

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
        } else if (mesh.name === 'RubberCover') {
            title = "Rubber Cover";
            const outerR = mesh.userData.coverRadiusOuter || 0;
            details = `Type: Rubber Lining<br>Thickness: ${(outerR - (outerR - 15)).toFixed(1)} mm`;
        } else if (mesh.userData.isShaft) {
            title = "Shaft";
            details = `Type: ${mesh.name}<br>Diameter: ${(mesh.geometry.parameters.radiusTop * 2).toFixed(1)} mm<br>Length: ${mesh.geometry.parameters.height.toFixed(1)} mm`;
        }

        this.infoPanel.innerHTML = `
            <strong style="font-size:16px; color:#00ddff;">${title}</strong><br><br>
            ${details}<br><br>
            <small style="color:#aaa;">Right-click to close</small>
        `;

        let left = screenX + 20;
        let top = screenY - 10;
        if (left + 280 > window.innerWidth) left = screenX - 280;
        if (top + 180 > window.innerHeight) top = screenY - 200;

        this.infoPanel.style.left = left + 'px';
        this.infoPanel.style.top = top + 'px';
        this.infoPanel.style.display = 'block';

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

    // ====================== LIGHTING ======================
    _setupLightsAndEnvironment: function () {
        this.lights.forEach(light => this.scene.remove(light));
        this.lights = [];

        // === DARK THEME BACKGROUND ===
        this.scene.background = new THREE.Color(0x0f172a);   // Deep navy

        // Environment map (still useful for reflections on metal)
        const envMap = new THREE.CubeTextureLoader().load([
            'https://raw.githubusercontent.com/mrdoob/three.js/dev/examples/textures/cube/Bridge2/posx.jpg',
            'https://raw.githubusercontent.com/mrdoob/three.js/dev/examples/textures/cube/Bridge2/negx.jpg',
            'https://raw.githubusercontent.com/mrdoob/three.js/dev/examples/textures/cube/Bridge2/posy.jpg',
            'https://raw.githubusercontent.com/mrdoob/three.js/dev/examples/textures/cube/Bridge2/negy.jpg',
            'https://raw.githubusercontent.com/mrdoob/three.js/dev/examples/textures/cube/Bridge2/posz.jpg',
            'https://raw.githubusercontent.com/mrdoob/three.js/dev/examples/textures/cube/Bridge2/negz.jpg'
        ]);
        this.scene.environment = envMap;

        // === DARK THEME LIGHTING ===

        // Ambient (soft base light)
        const ambient = new THREE.AmbientLight(0xffffff, 0.65);
        this.scene.add(ambient);

        // Hemisphere (sky/ground feel)
        const hemi = new THREE.HemisphereLight(0x64748b, 0x1e2937, 0.8);
        this.scene.add(hemi);

        // Key Light (main light - stronger for dark theme)
        const keyLight = new THREE.DirectionalLight(0xffffff, 1.75);
        keyLight.position.set(280, 520, 380);
        keyLight.castShadow = true;
        keyLight.shadow.mapSize.width = 2048;
        keyLight.shadow.mapSize.height = 2048;
        this.scene.add(keyLight);

        // Fill Light (cooler tone)
        const fillLight = new THREE.DirectionalLight(0xa5b4fc, 0.85);
        fillLight.position.set(-320, 340, -280);
        this.scene.add(fillLight);

        // Rim Light (very important in dark theme - defines edges)
        const rimLight = new THREE.DirectionalLight(0xbae6fd, 1.1);
        rimLight.position.set(80, 180, -480);
        this.scene.add(rimLight);

        // Extra top light for better cylinder definition
        const topLight = new THREE.DirectionalLight(0xffffff, 0.6);
        topLight.position.set(0, 650, 0);
        this.scene.add(topLight);

        console.log('✅ Dark theme lighting applied');
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
                if (this.composer) this.composer.setSize(w, h);
                if (this.fxaaPass) this.fxaaPass.uniforms['resolution'].value.set(1 / w, 1 / h);
            }
        };
        window.addEventListener('resize', handler);
    },

    debugPositions: function () {
        console.group('🔍 RollerViewer3D - Current Positions');
        if (this.assemblyGroup) {
            this.assemblyGroup.children.forEach(m => {
                console.log(`${m.name.padEnd(18)} | Pos: (${m.position.x.toFixed(1)}, ${m.position.y.toFixed(1)}, ${m.position.z.toFixed(1)})`);
            });
        } else {
            this.meshes.forEach(m => {
                console.log(`${m.name.padEnd(18)} | Pos: (${m.position.x.toFixed(1)}, ${m.position.y.toFixed(1)}, ${m.position.z.toFixed(1)})`);
            });
        }
        console.groupEnd();
    },

    dispose: function () {
        this.clearScene();
        if (this.renderer) this.renderer.dispose();
        console.log('🗑️ RollerViewer3D disposed');
    }
};