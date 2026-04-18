

// Direct Three.js viewer that works without BlazorThreeJS wrapper
window.RollerViewer3D = {
    scene: null,
    camera: null,
    renderer: null,
    controls: null,
    meshes: [],
    lights: [],
    isReady: false,

    init: function (containerElement, width, height) {
        try {
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
            this.renderer.toneMappingExposure = 1;
            containerElement.appendChild(this.renderer.domElement);

            // Create lights array to track them
            this.lights = [];

            // Ambient Light - base illumination
            const ambientLight = new THREE.AmbientLight(0xffffff, 0.8);
            this.scene.add(ambientLight);
            this.lights.push(ambientLight);
            console.log('✅ Ambient light added');

            // Point Light 1 - main light
            const pointLight1 = new THREE.PointLight(0xffffff, 3.8, 300);
            pointLight1.position.set(100, 100, 100);
            pointLight1.castShadow = true;
            pointLight1.shadow.mapSize.width = 2048;
            pointLight1.shadow.mapSize.height = 2048;
            pointLight1.shadow.camera.far = 500;
            this.scene.add(pointLight1);
            this.lights.push(pointLight1);
            console.log('✅ Point light 1 added');

            // Point Light 2 - fill light
            const pointLight2 = new THREE.PointLight(0xb0d0ff, 2.5, 250);
            pointLight2.position.set(-100, 80, -100);
            pointLight2.castShadow = true;
            pointLight2.shadow.mapSize.width = 2048;
            pointLight2.shadow.mapSize.height = 2048;
            pointLight2.shadow.camera.far = 500;
            this.scene.add(pointLight2);
            this.lights.push(pointLight2);
            console.log('✅ Point light 2 added');

            // Directional Light - overall direction
            const directionalLight = new THREE.DirectionalLight(0xffffff, 1.0);
            directionalLight.position.set(100, 150, 100);
            directionalLight.castShadow = true;
            directionalLight.shadow.mapSize.width = 2048;
            directionalLight.shadow.mapSize.height = 2048;
            directionalLight.shadow.camera.far = 500;
            directionalLight.shadow.camera.left = -500;
            directionalLight.shadow.camera.right = 500;
            directionalLight.shadow.camera.top = 500;
            directionalLight.shadow.camera.bottom = -500;
            this.scene.add(directionalLight);
            this.lights.push(directionalLight);
            console.log('✅ Directional light added');

            // Floor
            const floorGeometry = new THREE.PlaneGeometry(2000, 2000);
            const floorMaterial = new THREE.MeshStandardMaterial({ 
                color: 0x2a2a2a, 
                roughness: 0.8,
                metalness: 0.1,
                side: THREE.FrontSide
            });
            const floor = new THREE.Mesh(floorGeometry, floorMaterial);
            floor.receiveShadow = true;
            floor.castShadow = false;
            floor.rotation.x = -Math.PI / 2;
            floor.position.y = -50;
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
                this.controls.maxDistance = 1000;
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
                this.controls.maxDistance = 1000;
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

            // Create mesh
            const roller = new THREE.Mesh(geometry, material);
            roller.name = 'Roller';
            roller.castShadow = true;
            roller.receiveShadow = true;
            roller.position.set(0, length / 2, 0);
            roller.rotation.x = Math.PI / 2;
            roller.scale.set(scale, scale, scale);

            // Add to scene
            this.scene.add(roller);
            this.meshes.push(roller);

            console.log(`✅ Roller added to scene`);
            console.log(`   - Meshes: ${this.meshes.length}`);
            console.log(`   - Scene children: ${this.scene.children.length}`);
            console.log(`   - Lights in scene: ${this.lights.filter(l => this.scene.children.includes(l)).length}`);

            return true;
        } catch (error) {
            console.error('❌ Error adding roller:', error);
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




