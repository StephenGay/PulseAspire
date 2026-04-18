window.RollerViewer = {
    addRollerToScene: function (sceneName, diameter, length, scale) {
        try {
            console.log(`🔷 addRollerToScene called: diameter=${diameter}, length=${length}, scale=${scale}`);

            let scene = null;

            // Find the scene
            if (window.BlazorThreeJS && window.BlazorThreeJS.scene) {
                scene = window.BlazorThreeJS.scene;
                console.log('✅ Found scene in window.BlazorThreeJS.scene');
            } else {
                console.warn('⚠️ Could not find BlazorThreeJS.scene');
                return false;
            }

            if (!scene) {
                console.warn('⚠️ Scene object is null');
                return false;
            }

            // Check multiple locations for THREE.js
            let THREE = window.THREE;

            if (!THREE && typeof window.__THREE__ !== 'undefined') {
                THREE = window.__THREE__;
                console.log('✅ Found __THREE__ object');
                console.log('🔍 __THREE__ type:', typeof THREE);
                console.log('🔍 __THREE__ keys:', Object.keys(THREE).slice(0, 10));

                // Check if it's the actual THREE namespace
                if (THREE.CylinderGeometry) {
                    console.log('✅ __THREE__ has CylinderGeometry');
                } else if (THREE.THREE && THREE.THREE.CylinderGeometry) {
                    THREE = THREE.THREE;
                    console.log('✅ Found THREE.THREE with CylinderGeometry');
                } else if (THREE.default && THREE.default.CylinderGeometry) {
                    THREE = THREE.default;
                    console.log('✅ Found THREE.default with CylinderGeometry');
                } else {
                    console.warn('⚠️ __THREE__ does not have CylinderGeometry');
                    console.log('🔍 __THREE__ properties:', THREE);
                }
            }

            if (!THREE || typeof THREE.CylinderGeometry === 'undefined') {
                console.warn('⚠️ THREE.js CylinderGeometry not available, falling back to C#');
                return false;
            }

            console.log('✅ THREE.js library available, creating roller...');
            return window.RollerViewer.createAndAddRoller(scene, THREE, diameter, length, scale);
        } catch (error) {
            console.error('❌ Error in addRollerToScene: ' + error.message);
            console.error('Stack trace:', error.stack);
            return false;
        }
    },

    createAndAddRoller: function (scene, THREE, diameter, length, scale) {
        try {
            if (!THREE || typeof THREE.CylinderGeometry === 'undefined') {
                console.error('❌ THREE.js is not properly initialized');
                return false;
            }

            console.log('🔷 Creating roller geometry with THREE.js...');

            // Create the roller geometry
            const geometry = new THREE.CylinderGeometry(
                diameter / 2,  // radiusTop
                diameter / 2,  // radiusBottom
                length,        // height
                64,            // radialSegments
                4              // heightSegments
            );

            const material = new THREE.MeshStandardMaterial({
                color: 0xa8b5c0,
                metalness: 0.78,
                roughness: 0.32
            });

            const roller = new THREE.Mesh(geometry, material);
            roller.name = 'Roller';
            roller.position.set(0, length / 2, 0);
            roller.rotation.x = Math.PI / 2;
            roller.scale.set(scale, scale, scale);

            console.log(`🔷 Scene children before: ${scene.children.length}`);

            scene.add(roller);
            console.log('✅ Roller mesh added directly to Three.js scene');
            console.log(`🔷 Scene now has ${scene.children.length} children`);

            // The BlazorThreeJS animation loop will render the changes automatically
            return true;
        } catch (error) {
            console.error('❌ Error in createAndAddRoller: ' + error.message);
            console.error('Stack trace:', error.stack);
            return false;
        }
    }
};






