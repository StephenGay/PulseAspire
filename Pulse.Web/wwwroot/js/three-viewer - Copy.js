

// Direct Three.js viewer that works without BlazorThreeJS wrapper
window.RollerViewer3D = {
    scene: null,
    camera: null,
    renderer: null,
    controls: null,
    meshes: [],
    lights: [],
    isReady: false,
    animateRoller: false,

    init: function (containerElement, width, height) {
        try {
            // Get actual container dimensions if not provided
            if (!width || !height || width <= 0 || height <= 0) {
                width = containerElement.clientWidth;
                height = containerElement.clientHeight;
                console.log(`📐 Using container dimensions: ${width}x${height}`);
            }

            // Ensure THREE is loaded before proceeding
            if (typeof THREE === 'undefined' || !THREE.Scene) {
                console.log('⏳ Waiting for THREE.js to load...');
                return this._waitForTHREE(() => this._doInit(containerElement, width, height));
            }

            return this._doInit(containerElement, width, height);
        } catch (error) {
            console.error('❌ Error initializing viewer:', error);
            return false;
        }
    },

    _waitForTHREE: function (callback) {
        return new Promise((resolve) => {
            let attempts = 0;
            const maxAttempts = 100;

            const checkTHREE = setInterval(() => {
                if (typeof THREE !== 'undefined' && THREE.Scene && THREE.WebGLRenderer) {
                    clearInterval(checkTHREE);
                    console.log('✅ THREE.js loaded, initializing...');
                    resolve(callback());
                } else {
                    attempts++;
                    if (attempts % 10 === 0) {
                        console.log(`⏳ Waiting for THREE.js... (${attempts}/${maxAttempts})`);
                    }
                    if (attempts >= maxAttempts) {
                        clearInterval(checkTHREE);
                        console.error('❌ THREE.js failed to load after timeout');
                        resolve(false);
                    }
                }
            }, 50);
        });
    },

    _doInit: function (containerElement, width, height) {
        try {
            console.log('🔷 Initializing Three.js viewer...');

            // Scene
            this.scene = new THREE.Scene();
            this.scene.background = new THREE.Color(0x000000);
            this.scene.fog = null;

            // Camera
            this.camera = new THREE.PerspectiveCamera(75, width / height, 0.1, 10000);
            this.camera.position.set(0, 100, 250);
            this.camera.lookAt(0, 0, 0);

            // Renderer
            this.renderer = new THREE.WebGLRenderer({ antialias: true, alpha: true, preserveDrawingBuffer: true });
            this.renderer.setSize(width, height);
            this.renderer.setPixelRatio(window.devicePixelRatio);
            this.renderer.shadowMap.enabled = true;
            this.renderer.shadowMap.type = THREE.PCFShadowMap;
            this.renderer.outputColorSpace = THREE.SRGBColorSpace;
            this.renderer.toneMapping = THREE.ACESFilmicToneMapping;
            this.renderer.toneMappingExposure = 1.2;
            containerElement.appendChild(this.renderer.domElement);

            // Create lights array to track them
            this.lights = [];

            // Ambient Light - soft base illumination
            const ambientLight = new THREE.AmbientLight(0xffffff, 0.4);
            this.scene.add(ambientLight);
            this.lights.push(ambientLight);

            // Hemisphere Light - natural sky/ground gradient
            const hemiLight = new THREE.HemisphereLight(0xddeeff, 0x8899aa, 0.6);
            hemiLight.position.set(0, 200, 0);
            this.scene.add(hemiLight);
            this.lights.push(hemiLight);

            // Key Light - main directional light from upper-front-right
            const keyLight = new THREE.DirectionalLight(0xffffff, 1.2);
            keyLight.position.set(300, 400, 300);
            keyLight.castShadow = true;
            keyLight.shadow.mapSize.width = 2048;
            keyLight.shadow.mapSize.height = 2048;
            keyLight.shadow.camera.far = 2000;
            keyLight.shadow.camera.left = -1000;
            keyLight.shadow.camera.right = 1000;
            keyLight.shadow.camera.top = 1000;
            keyLight.shadow.camera.bottom = -1000;
            this.scene.add(keyLight);
            this.lights.push(keyLight);

            // Fill Light - softer from opposite side to reduce harsh shadows
            const fillLight = new THREE.DirectionalLight(0xc0d8ff, 0.5);
            fillLight.position.set(-300, 200, -200);
            this.scene.add(fillLight);
            this.lights.push(fillLight);

            // Rim Light - subtle backlight for edge definition
            const rimLight = new THREE.DirectionalLight(0xffffff, 0.3);
            rimLight.position.set(0, 100, -400);
            this.scene.add(rimLight);
            this.lights.push(rimLight);

            console.log('✅ Lights added (ambient + hemisphere + key + fill + rim)');

            // Floor
            const floorGeometry = new THREE.PlaneGeometry(10000, 10000);
            const floorMaterial = new THREE.MeshStandardMaterial({ 
                color: 0xe6e6e6, 
                roughness: 0.8,
                metalness: 0.1,
                side: THREE.FrontSide
            });
            const floor = new THREE.Mesh(floorGeometry, floorMaterial);
            floor.receiveShadow = true;
            floor.castShadow = false;
            floor.rotation.x = -Math.PI / 2;
            floor.position.y = -200;
            this.scene.add(floor);
            console.log('✅ Floor created');

            // Initialize OrbitControls immediately
            this._initializeOrbitControls();

            // Handle resize
            window.addEventListener('resize', () => this.onWindowResize(containerElement));

            // Start animation loop
            this.animate();

            this.isReady = true;
            console.log('✅ Three.js viewer initialized - Ready for meshes');
            console.log(`   Lights: ${this.lights.length}, Scene children: ${this.scene.children.length}`);
            return true;
        } catch (error) {
            console.error('❌ Error in _doInit:', error);
            console.error(error.stack);
            return false;
        }
    },

    _initializeOrbitControls: function () {
        try {
            // Check if OrbitControls is available
            if (typeof OrbitControls !== 'undefined') {
                console.log('✅ OrbitControls found, initializing...');
                this.controls = new OrbitControls(this.camera, this.renderer.domElement);
                this.controls.enableDamping = true;
                this.controls.dampingFactor = 0.05;
                this.controls.autoRotate = false;
                this.controls.enableZoom = true;
                this.controls.autoRotateSpeed = 0;
                this.controls.minDistance = 50;
                this.controls.maxDistance = 5000;
                console.log('✅ OrbitControls fully initialized');
                return true;
            } else {
                console.warn('⚠️ OrbitControls not available yet');
                // Retry after a delay
                setTimeout(() => this._retryOrbitControls(), 200);
                return false;
            }
        } catch (error) {
            console.error('❌ Error initializing OrbitControls:', error);
            return false;
        }
    },

    _retryOrbitControls: function () {
        if (this.controls) return; // Already initialized

        if (typeof OrbitControls !== 'undefined' && this.camera && this.renderer) {
            console.log('🔄 Retrying OrbitControls initialization...');
            try {
                this.controls = new OrbitControls(this.camera, this.renderer.domElement);
                this.controls.enableDamping = true;
                this.controls.dampingFactor = 0.05;
                this.controls.autoRotate = false;
                this.controls.enableZoom = true;
                this.controls.minDistance = 50;
                this.controls.maxDistance = 10000;
                console.log('✅ OrbitControls initialized on retry');
            } catch (error) {
                console.error('❌ Retry failed:', error);
            }
        } else {
            console.warn('⚠️ Conditions not met, will retry again...');
            setTimeout(() => this._retryOrbitControls(), 300);
        }
    },

    addRoller: function (diameter, length, scale) {
        try {
            if (typeof THREE === 'undefined') {
                console.error('❌ THREE.js not available');
                return false;
            }

            if (!this.isReady) {
                console.error('❌ Viewer not initialized');
                return false;
            }

            // Clear any existing meshes
            this.clearMeshes();

            console.log(`🔷 Adding roller: diameter=${diameter}, length=${length}, scale=${scale}`);

            // Create cylinder geometry for roller
            const geometry = new THREE.CylinderGeometry(
                diameter / 2,  // radiusTop
                diameter / 2,  // radiusBottom
                length,        // height
                64,            // radialSegments
                8              // heightSegments
            );

            // Create material with good lighting response
            const material = new THREE.MeshStandardMaterial({
                color: 0xa8b5c0,
                metalness: 0.65,
                roughness: 0.35,
                side: THREE.FrontSide,
                flatShading: false,
                wireframe: false
            });

            // Create mesh - NO SCALING YET
            const roller = new THREE.Mesh(geometry, material);
            roller.name = 'Roller';
            roller.castShadow = true;
            roller.receiveShadow = true;
            // Position BEFORE rotating - at origin, along Y axis (vertical)
            roller.position.set(0, 0, 0);
            // NO SCALE YET - scale will be applied after cover is added

            // Add to scene
            this.scene.add(roller);
            this.meshes.push(roller);

            console.log(`✅ Roller added to scene`);
            console.log(`   - Meshes: ${this.meshes.length}`);
            console.log(`   - Scene children: ${this.scene.children.length}`);

            return true;
        } catch (error) {
            console.error('❌ Error adding roller:', error);
            console.error(error.stack);
            return false;
        }
    },

    applyScalingAndPositioning: function (scale) {
        try {
            if (this.meshes.length < 2) {
                console.error('❌ Roller and cover must be added first');
                return false;
            }

            const roller = this.meshes[0];
            const cover = this.meshes[1];

            console.log(`🔷 Applying scale and positioning: scale=${scale}`);

            // Apply scale to both
            //roller.scale.set(scale, scale, scale);
            //cover.scale.set(scale, scale, scale);

            // Now rotate both to horizontal position
            //roller.rotation.x = Math.PI / 2;
            //cover.rotation.x = Math.PI / 2;

            // Get dimensions for camera positioning
            const bbox = new THREE.Box3().setFromObject(roller);
            const size = bbox.getSize(new THREE.Vector3());
            const maxDim = Math.max(size.x, size.y, size.z);

            const fov = this.camera.fov * (Math.PI / 180);
            let cameraDistance = Math.abs(maxDim / 2 / Math.tan(fov / 2));
            cameraDistance *= 1.5;

            console.log(`📐 Roller size: ${size.x.toFixed(2)} x ${size.y.toFixed(2)} x ${size.z.toFixed(2)}`);
            console.log(`📐 Max dimension: ${maxDim.toFixed(2)}`);
            console.log(`📐 Calculated camera distance: ${cameraDistance.toFixed(2)}`);

            // Position camera to view from a good angle
            const angle = Math.PI / 4;
            const cameraHeight = maxDim * 0.6;
            const cameraX = Math.cos(angle) * cameraDistance;
            const cameraZ = Math.sin(angle) * cameraDistance;

            this.setCameraPosition(cameraX, cameraHeight, cameraZ);

            // Lift everything off the floor BEFORE rotating
            const rollerRadius = roller.geometry.parameters.radiusTop;
            //const coverRadiusOuter = rollerRadius + cover.geometry.parameters.coverRadiusOuter;
            //const liftHeight = coverRadiusOuter + 50;

             // liftHeight;
            //cover.position.y = cover.position.y + 50; // liftHeight;

            // Now rotate both to horizontal position
            // Both are along Y axis, so rotate on X axis
            roller.rotation.x = Math.PI / 2;
            cover.rotation.x = Math.PI / 2;

            // Lift both off the floor after rotation
            // After X rotation, the radius extends in Y, so lift by the cover's outer radius
            const liftHeight = cover.userData.coverRadiusOuter + 10;
            roller.position.y = liftHeight;
            cover.position.y = liftHeight;

            // Debug: Log all coordinates after rotation
            const rollerBbox = new THREE.Box3().setFromObject(roller);
            const rollerMin = rollerBbox.min;
            const rollerMax = rollerBbox.max;
            console.log(`📐 DEBUG - Roller after rotation:`);
            console.log(`   Position: (${roller.position.x.toFixed(2)}, ${roller.position.y.toFixed(2)}, ${roller.position.z.toFixed(2)})`);
            console.log(`   Rotation: (${roller.rotation.x.toFixed(4)}, ${roller.rotation.y.toFixed(4)}, ${roller.rotation.z.toFixed(4)})`);
            console.log(`   BBox min: (${rollerMin.x.toFixed(2)}, ${rollerMin.y.toFixed(2)}, ${rollerMin.z.toFixed(2)})`);
            console.log(`   BBox max: (${rollerMax.x.toFixed(2)}, ${rollerMax.y.toFixed(2)}, ${rollerMax.z.toFixed(2)})`);
            console.log(`   Roller radius: ${roller.geometry.parameters.radiusTop}`);
            console.log(`   Roller length: ${roller.geometry.parameters.height}`);

            const coverBbox = new THREE.Box3().setFromObject(cover);
            const coverMin = coverBbox.min;
            const coverMax = coverBbox.max;
            console.log(`📐 DEBUG - Cover after rotation:`);
            console.log(`   Position: (${cover.position.x.toFixed(2)}, ${cover.position.y.toFixed(2)}, ${cover.position.z.toFixed(2)})`);
            console.log(`   Rotation: (${cover.rotation.x.toFixed(4)}, ${cover.rotation.y.toFixed(4)}, ${cover.rotation.z.toFixed(4)})`);
            console.log(`   BBox min: (${coverMin.x.toFixed(2)}, ${coverMin.y.toFixed(2)}, ${coverMin.z.toFixed(2)})`);
            console.log(`   BBox max: (${coverMax.x.toFixed(2)}, ${coverMax.y.toFixed(2)}, ${coverMax.z.toFixed(2)})`);
            console.log(`   Cover userData.leftOffset: ${cover.userData.leftOffset}`);

            console.log(`📐 DEBUG - Overlap check:`);
            console.log(`   Roller X range: ${rollerMin.x.toFixed(2)} to ${rollerMax.x.toFixed(2)} (diameter: ${(rollerMax.x - rollerMin.x).toFixed(2)})`);
            console.log(`   Cover  X range: ${coverMin.x.toFixed(2)} to ${coverMax.x.toFixed(2)} (diameter: ${(coverMax.x - coverMin.x).toFixed(2)})`);
            console.log(`   Roller Z range: ${rollerMin.z.toFixed(2)} to ${rollerMax.z.toFixed(2)} (length: ${(rollerMax.z - rollerMin.z).toFixed(2)})`);
            console.log(`   Cover  Z range: ${coverMin.z.toFixed(2)} to ${coverMax.z.toFixed(2)} (length: ${(coverMax.z - coverMin.z).toFixed(2)})`);

            // Enable slow auto-rotate and roller spin
            //if (this.controls) {
            //    this.controls.autoRotate = true;
            //    this.controls.autoRotateSpeed = 1.0;
            //}
            this.animateRoller = true;

            console.log(`✅ Scale and positioning applied`);

            return true;
        } catch (error) {
            console.error('❌ Error applying scaling and positioning:', error);
            console.error(error.stack);
            return false;
        }
    },

    addRubberCover: function (leftOffset, coverLength, coverThickness, coverColor) {
        try {
            if (typeof THREE === 'undefined') {
                console.error('❌ THREE.js not available');
                return false;
            }

            if (!this.isReady) {
                console.error('❌ Viewer not initialized');
                return false;
            }

            if (this.meshes.length === 0) {
                console.error('❌ No roller in scene to add cover to');
                return false;
            }

            console.log(`🔷 Adding rubber cover: offset=${leftOffset}, length=${coverLength}, thickness=${coverThickness}, color=${coverColor}`);

            // Get the roller mesh to match its dimensions
            const roller = this.meshes[0];
            const rollerRadius = roller.geometry.parameters.radiusTop;
            const rollerLength = roller.geometry.parameters.height;
            // Cover starts at shell radius and extends by thickness (already radius difference)
            const coverRadiusInner = rollerRadius;
            const coverRadiusOuter = rollerRadius + coverThickness;

            console.log(`📐 Cover radii - Inner: ${coverRadiusInner}, Outer: ${coverRadiusOuter}, Roller radius: ${rollerRadius}`);

            // Create rubber cover with visible thickness using a Group
            // Outer cylinder (open-ended) - the visible cover surface
            const outerGeometry = new THREE.CylinderGeometry(
                coverRadiusOuter, coverRadiusOuter, coverLength, 64, 1, true
            );
            // Inner cylinder (open-ended) - matches roller surface exactly
            const innerGeometry = new THREE.CylinderGeometry(
                coverRadiusInner, coverRadiusInner, coverLength, 64, 1, true
            );
            // Top ring cap - shows thickness cross-section
            const topRing = new THREE.RingGeometry(coverRadiusInner, coverRadiusOuter, 64);
            // Bottom ring cap - shows thickness cross-section
            const bottomRing = new THREE.RingGeometry(coverRadiusInner, coverRadiusOuter, 64);

            // Create material for rubber cover
            const coverMaterial = new THREE.MeshStandardMaterial({
                color: coverColor || 0x000000,
                metalness: 0.1,
                roughness: 0.8,
                side: THREE.DoubleSide,
                flatShading: false
            });

            // Build cover group with all 4 surfaces to form a solid sleeve (no gaps)
            const coverGroup = new THREE.Group();
            coverGroup.name = 'RubberCover';

            const outerMesh = new THREE.Mesh(outerGeometry, coverMaterial);
            outerMesh.castShadow = true;
            outerMesh.receiveShadow = true;
            coverGroup.add(outerMesh);

            const innerMesh = new THREE.Mesh(innerGeometry, coverMaterial);
            innerMesh.castShadow = true;
            innerMesh.receiveShadow = true;
            coverGroup.add(innerMesh);

            // Top cap - rotate to face up and position at top
            const topMesh = new THREE.Mesh(topRing, coverMaterial);
            topMesh.rotation.x = -Math.PI / 2;
            topMesh.position.y = coverLength / 2;
            coverGroup.add(topMesh);

            // Bottom cap - rotate to face down and position at bottom
            const bottomMesh = new THREE.Mesh(bottomRing, coverMaterial);
            bottomMesh.rotation.x = Math.PI / 2;
            bottomMesh.position.y = -coverLength / 2;
            coverGroup.add(bottomMesh);

            // Calculate offset from center of roller
            // leftOffset=0 means centered, positive values shift along the roller axis
            const offsetFromCenter = leftOffset;

            // Position the cover group at the correct offset along the roller
            coverGroup.position.set(0, offsetFromCenter, 0);

            // Store data for reference
            coverGroup.userData.leftOffset = leftOffset;
            coverGroup.userData.coverRadiusOuter = coverRadiusOuter;

            // Add to scene
            this.scene.add(coverGroup);
            this.meshes.push(coverGroup);

            console.log(`✅ Rubber cover added to scene`);
            console.log(`   - Roller radius (inner): ${rollerRadius.toFixed(2)}`);
            console.log(`   - Cover outer radius: ${coverRadiusOuter.toFixed(2)}`);
            console.log(`   - Cover thickness: ${coverThickness.toFixed(2)}`);
            console.log(`   - Cover length: ${coverLength.toFixed(2)}`);
            console.log(`   - Left offset: ${leftOffset.toFixed(2)}`);
            console.log(`   - Total meshes: ${this.meshes.length}`);

            return true;
        } catch (error) {
            console.error('❌ Error adding rubber cover:', error);
            console.error(error.stack);
            return false;
        }
    },

    setCameraPosition: function (x, y, z) {
        try {
            this.camera.position.set(x, y, z);
            this.camera.lookAt(0, 0, 0);
            if (this.controls) {
                this.controls.target.set(0, 0, 0);
                this.controls.update();
            }
            console.log(`🔷 Camera positioned at: ${x}, ${y}, ${z}`);
            return true;
        } catch (error) {
            console.error('❌ Error setting camera position:', error);
            return false;
        }
    },

    clearMeshes: function () {
        try {
            this.meshes.forEach(mesh => this.scene.remove(mesh));
            this.meshes = [];
            console.log('🔷 Meshes cleared');
            return true;
        } catch (error) {
            console.error('❌ Error clearing meshes:', error);
            return false;
        }
    },

    animate: function () {
        requestAnimationFrame(() => this.animate());

        // Spin the roller and cover around their length axis (Z after rotation)
        if (this.animateRoller && this.meshes.length >= 2) {
            const spinSpeed = 0.003;
            this.meshes[0].rotation.z += spinSpeed;
            this.meshes[1].rotation.z += spinSpeed;
        }

        if (this.controls && this.controls.update) {
            this.controls.update();
        }

        if (this.renderer && this.scene && this.camera) {
            this.renderer.render(this.scene, this.camera);
        }
    },

    onWindowResize: function (container) {
        try {
            const width = container.clientWidth;
            const height = container.clientHeight;

            if (width > 0 && height > 0) {
                this.camera.aspect = width / height;
                this.camera.updateProjectionMatrix();
                this.renderer.setSize(width, height);
            }
        } catch (error) {
            console.error('❌ Error on window resize:', error);
        }
    }
};

// Legacy init function for backwards compatibility
window.initThreeViewer = function (container, modelData) {
    const viewer = window.RollerViewer3D;
    const width = container.clientWidth || window.innerWidth;
    const height = container.clientHeight || 800;

    viewer.init(container, width, height);

    if (modelData && modelData.geometry) {
        const geometry = new THREE.BufferGeometry();
        geometry.setAttribute('position', new THREE.BufferAttribute(new Float32Array(modelData.geometry.vertices), 3));
        geometry.setAttribute('normal', new THREE.BufferAttribute(new Float32Array(modelData.geometry.normals), 3));
        geometry.setIndex(new THREE.BufferAttribute(new Uint32Array(modelData.geometry.indices), 1));

        const material = new THREE.MeshPhongMaterial({
            color: modelData.material.color,
            roughness: modelData.material.roughness,
            metalness: modelData.material.metalness
        });

        const mesh = new THREE.Mesh(geometry, material);
        viewer.scene.add(mesh);
        viewer.meshes.push(mesh);
    }
};




