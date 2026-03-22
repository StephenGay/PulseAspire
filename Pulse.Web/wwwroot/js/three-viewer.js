window.initThreeViewer = function (container, modelData) {
    const scene = new THREE.Scene();
    const camera = new THREE.PerspectiveCamera(75, window.innerWidth / window.innerHeight, 0.1, 1000);
    const renderer = new THREE.WebGLRenderer({ antialias: true, alpha: true });

    renderer.setSize(container.clientWidth, container.clientHeight);
    renderer.setClearColor(0x1a1a1a);
    container.appendChild(renderer.domElement);

    // Create geometry from model data
    const geometry = new THREE.BufferGeometry();
    geometry.setAttribute('position', new THREE.BufferAttribute(new Float32Array(modelData.geometry.vertices), 3));
    geometry.setAttribute('normal', new THREE.BufferAttribute(new Float32Array(modelData.geometry.normals), 3));
    geometry.setIndex(new THREE.BufferAttribute(new Uint32Array(modelData.geometry.indices), 1));

    // Create material
    const material = new THREE.MeshPhongMaterial({
        color: modelData.material.color,
        roughness: modelData.material.roughness,
        metalness: modelData.material.metalness
    });

    const mesh = new THREE.Mesh(geometry, material);
    scene.add(mesh);

    // Lighting
    const light1 = new THREE.DirectionalLight(0xffffff, 0.8);
    light1.position.set(5, 10, 7);
    scene.add(light1);

    const light2 = new THREE.AmbientLight(0xffffff, 0.4);
    scene.add(light2);

    camera.position.z = 150;

    // Animation loop
    function animate() {
        requestAnimationFrame(animate);
        mesh.rotation.y += 0.005;
        renderer.render(scene, camera);
    }
    animate();
};