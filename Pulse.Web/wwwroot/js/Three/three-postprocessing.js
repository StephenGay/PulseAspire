/**
 * Three.js Post-Processing Bundle (r134) - FINAL COMPLETE VERSION
 * Includes: FullScreenQuad + CopyShader + EffectComposer + RenderPass + UnrealBloomPass + ShaderPass + FXAAShader
 */

// ====================== FULL SCREEN QUAD ======================
THREE.FullScreenQuad = (function () {
    var camera = new THREE.OrthographicCamera(-1, 1, 1, -1, 0, 1);
    var geometry = new THREE.BufferGeometry();
    geometry.setAttribute('position', new THREE.Float32BufferAttribute([-1, 3, 0, -1, -1, 0, 3, -1, 0], 3));
    geometry.setAttribute('uv', new THREE.Float32BufferAttribute([0, 2, 0, 0, 2, 0], 2));

    var FullScreenQuad = function (material) {
        this._mesh = new THREE.Mesh(geometry, material);
    };

    Object.defineProperty(FullScreenQuad.prototype, 'material', {
        get: function () { return this._mesh.material; },
        set: function (value) { this._mesh.material = value; }
    });

    Object.assign(FullScreenQuad.prototype, {
        dispose: function () {
            this._mesh.geometry.dispose();
        },
        render: function (renderer) {
            renderer.render(this._mesh, camera);
        }
    });

    return FullScreenQuad;
})();

// ====================== COPY SHADER (REQUIRED) ======================
THREE.CopyShader = {
    uniforms: {
        "tDiffuse": { value: null },
        "opacity": { value: 1.0 }
    },
    vertexShader: `varying vec2 vUv;
    void main() {
        vUv = uv;
        gl_Position = projectionMatrix * modelViewMatrix * vec4( position, 1.0 );
    }`,
    fragmentShader: `uniform float opacity;
    uniform sampler2D tDiffuse;
    varying vec2 vUv;
    void main() {
        vec4 texel = texture2D( tDiffuse, vUv );
        gl_FragColor = opacity * texel;
    }`
};

// ====================== EFFECT COMPOSER ======================
THREE.EffectComposer = function (renderer, renderTarget) {
    this.renderer = renderer;
    if (renderTarget === undefined) {
        var size = renderer.getSize(new THREE.Vector2());
        this._pixelRatio = renderer.getPixelRatio();
        this._width = size.width;
        this._height = size.height;
        renderTarget = new THREE.WebGLRenderTarget(this._width * this._pixelRatio, this._height * this._pixelRatio);
        renderTarget.texture.name = 'EffectComposer.rt1';
    } else {
        this._pixelRatio = 1;
        this._width = renderTarget.width;
        this._height = renderTarget.height;
    }
    this.renderTarget1 = renderTarget;
    this.renderTarget2 = renderTarget.clone();
    this.renderTarget2.texture.name = 'EffectComposer.rt2';
    this.writeBuffer = this.renderTarget1;
    this.readBuffer = this.renderTarget2;
    this.renderToScreen = true;
    this.passes = [];
    this.copyPass = new THREE.ShaderPass(THREE.CopyShader);
    this.copyPass.material.blending = THREE.NoBlending;
};

THREE.EffectComposer.prototype = {
    swapBuffers: function () {
        var tmp = this.readBuffer;
        this.readBuffer = this.writeBuffer;
        this.writeBuffer = tmp;
    },
    addPass: function (pass) {
        this.passes.push(pass);
        pass.setSize(this._width * this._pixelRatio, this._height * this._pixelRatio);
    },
    insertPass: function (pass, index) {
        this.passes.splice(index, 0, pass);
        pass.setSize(this._width * this._pixelRatio, this._height * this._pixelRatio);
    },
    removePass: function (pass) {
        var index = this.passes.indexOf(pass);
        if (index !== -1) {
            this.passes.splice(index, 1);
        }
    },
    isLastPass: function (passIndex) {
        return passIndex === this.passes.length - 1;
    },
    render: function (deltaTime) {
        var currentRenderTarget = this.renderer.getRenderTarget();
        var maskActive = false;
        var pass, i, il = this.passes.length;
        for (i = 0; i < il; i++) {
            pass = this.passes[i];
            if (pass.enabled === false) continue;
            pass.renderToScreen = (this.renderToScreen && this.isLastPass(i));
            pass.render(this.renderer, this.writeBuffer, this.readBuffer, deltaTime, maskActive);
            if (pass.needsSwap) {
                if (maskActive) {
                    var context = this.renderer.getContext();
                    var stencil = this.renderer.state.buffers.stencil;
                    stencil.setFunc(context.NOTEQUAL);
                    this.copyPass.render(this.renderer, this.writeBuffer, this.readBuffer, deltaTime);
                    stencil.setFunc(context.ALWAYS);
                }
                this.swapBuffers();
            }
            if (pass instanceof THREE.MaskPass) {
                maskActive = true;
            } else if (pass instanceof THREE.ClearMaskPass) {
                maskActive = false;
            }
        }
        this.renderer.setRenderTarget(currentRenderTarget);
    },
    reset: function (renderTarget) {
        if (renderTarget === undefined) {
            var size = this.renderer.getSize(new THREE.Vector2());
            this._pixelRatio = this.renderer.getPixelRatio();
            this._width = size.width;
            this._height = size.height;
            renderTarget = this.renderTarget1.clone();
            renderTarget.setSize(this._width * this._pixelRatio, this._height * this._pixelRatio);
        }
        this.renderTarget1.dispose();
        this.renderTarget2.dispose();
        this.renderTarget1 = renderTarget;
        this.renderTarget2 = renderTarget.clone();
        this.writeBuffer = this.renderTarget1;
        this.readBuffer = this.renderTarget2;
    },
    setSize: function (width, height) {
        this._width = width;
        this._height = height;
        var effectiveWidth = this._width * this._pixelRatio;
        var effectiveHeight = this._height * this._pixelRatio;
        this.renderTarget1.setSize(effectiveWidth, effectiveHeight);
        this.renderTarget2.setSize(effectiveWidth, effectiveHeight);
        for (var i = 0; i < this.passes.length; i++) {
            this.passes[i].setSize(effectiveWidth, effectiveHeight);
        }
    },
    setPixelRatio: function (pixelRatio) {
        this._pixelRatio = pixelRatio;
        this.setSize(this._width, this._height);
    }
};

// ====================== RENDER PASS ======================
THREE.RenderPass = function (scene, camera, overrideMaterial, clearColor, clearAlpha) {
    this.scene = scene;
    this.camera = camera;
    this.overrideMaterial = overrideMaterial;
    this.clearColor = clearColor;
    this.clearAlpha = (clearAlpha !== undefined) ? clearAlpha : 0;
    this.clear = true;
    this.clearDepth = false;
    this.needsSwap = false;
};

THREE.RenderPass.prototype = {
    render: function (renderer, writeBuffer, readBuffer, deltaTime, maskActive) {
        var oldAutoClear = renderer.autoClear;
        renderer.autoClear = false;
        var oldClearColor, oldClearAlpha;
        if (this.clearColor) {
            oldClearColor = renderer.getClearColor().getHex();
            oldClearAlpha = renderer.getClearAlpha();
            renderer.setClearColor(this.clearColor, this.clearAlpha);
        }
        if (this.clearDepth) {
            renderer.clearDepth();
        }
        this.scene.overrideMaterial = this.overrideMaterial;
        renderer.setRenderTarget(this.renderToScreen ? null : readBuffer);
        if (this.clear) renderer.clear(renderer.autoClearColor, renderer.autoClearDepth, renderer.autoClearStencil);
        renderer.render(this.scene, this.camera);
        this.scene.overrideMaterial = null;
        if (this.clearColor) {
            renderer.setClearColor(oldClearColor, oldClearAlpha);
        }
        renderer.autoClear = oldAutoClear;
    }
};

// ====================== UNREAL BLOOM PASS ======================
THREE.UnrealBloomPass = function (resolution, strength, radius, threshold) {
    this.strength = (strength !== undefined) ? strength : 1;
    this.radius = radius;
    this.threshold = threshold;
    this.resolution = (resolution !== undefined) ? new THREE.Vector2(resolution.x, resolution.y) : new THREE.Vector2(256, 256);
    this.clearColor = new THREE.Color(0, 0, 0);
    var pars = { minFilter: THREE.LinearFilter, magFilter: THREE.LinearFilter, format: THREE.RGBAFormat };
    this.renderTargetsHorizontal = [];
    this.renderTargetsVertical = [];
    this.nMips = 5;
    var resx = Math.round(this.resolution.x / 2);
    var resy = Math.round(this.resolution.y / 2);
    for (var i = 0; i < this.nMips; i++) {
        var renderTargetHorizonal = new THREE.WebGLRenderTarget(resx, resy, pars);
        renderTargetHorizonal.texture.name = 'UnrealBloomPass.h' + i;
        renderTargetHorizonal.texture.generateMipmaps = false;
        this.renderTargetsHorizontal.push(renderTargetHorizonal);
        var renderTargetVertical = new THREE.WebGLRenderTarget(resx, resy, pars);
        renderTargetVertical.texture.name = 'UnrealBloomPass.v' + i;
        renderTargetVertical.texture.generateMipmaps = false;
        this.renderTargetsVertical.push(renderTargetVertical);
        resx = Math.round(resx / 2);
        resy = Math.round(resy / 2);
    }
    this.highPassUniforms = {
        "colorTexture": { value: null },
        "threshold": { value: 1.0 },
        "exposure": { value: 1.0 }
    };
    this.materialHighPassFilter = new THREE.ShaderMaterial({
        uniforms: this.highPassUniforms,
        vertexShader: `varying vec2 vUv;
        void main() {
            vUv = uv;
            gl_Position = projectionMatrix * modelViewMatrix * vec4( position, 1.0 );
        }`,
        fragmentShader: `uniform sampler2D colorTexture;
        uniform float threshold;
        varying vec2 vUv;
        void main() {
            vec4 texel = texture2D( colorTexture, vUv );
            vec3 luma = vec3( 0.299, 0.587, 0.114 );
            float v = dot( texel.xyz, luma );
            vec4 outputColor = vec4( texel.rgb, texel.a );
            if ( v < threshold ) outputColor = vec4( 0.0 );
            gl_FragColor = outputColor;
        }`
    });
    this.separableBlurMaterials = [];
    var kernelSizeArray = [3, 5, 7, 9, 11];
    var resx = Math.round(this.resolution.x / 2);
    var resy = Math.round(this.resolution.y / 2);
    for (var i = 0; i < this.nMips; i++) {
        this.separableBlurMaterials.push(this.getSeperableBlurMaterial(kernelSizeArray[i]));
        this.separableBlurMaterials[i].uniforms["colorTexture"].value = this.renderTargetsHorizontal[i].texture;
        this.separableBlurMaterials[i].uniforms["invSize"].value = new THREE.Vector2(1 / resx, 1 / resy);
        resx = Math.round(resx / 2);
        resy = Math.round(resy / 2);
    }
    this.compositeMaterial = this.getCompositeMaterial(this.nMips);
    this.compositeMaterial.uniforms["blurTexture1"].value = this.renderTargetsVertical[0].texture;
    this.compositeMaterial.uniforms["blurTexture2"].value = this.renderTargetsVertical[1].texture;
    this.compositeMaterial.uniforms["blurTexture3"].value = this.renderTargetsVertical[2].texture;
    this.compositeMaterial.uniforms["blurTexture4"].value = this.renderTargetsVertical[3].texture;
    this.compositeMaterial.uniforms["blurTexture5"].value = this.renderTargetsVertical[4].texture;
    this.compositeMaterial.uniforms["bloomStrength"].value = strength;
    this.compositeMaterial.uniforms["bloomRadius"].value = 0.1;
    this.compositeMaterial.needsUpdate = true;
    var bloomFactors = [1.0, 0.8, 0.6, 0.4, 0.2];
    this.compositeMaterial.uniforms["bloomFactors"].value = bloomFactors;
    this.bloomTintColors = [new THREE.Vector3(1, 1, 1), new THREE.Vector3(1, 1, 1), new THREE.Vector3(1, 1, 1), new THREE.Vector3(1, 1, 1), new THREE.Vector3(1, 1, 1)];
    this.compositeMaterial.uniforms["bloomTintColors"].value = this.bloomTintColors;
    this.copyUniforms = {
        "tDiffuse": { value: null },
        "opacity": { value: 1.0 }
    };
    this.materialCopy = new THREE.ShaderMaterial({
        uniforms: this.copyUniforms,
        vertexShader: `varying vec2 vUv;
        void main() {
            vUv = uv;
            gl_Position = projectionMatrix * modelViewMatrix * vec4( position, 1.0 );
        }`,
        fragmentShader: `uniform float opacity;
        uniform sampler2D tDiffuse;
        varying vec2 vUv;
        void main() {
            vec4 texel = texture2D( tDiffuse, vUv );
            gl_FragColor = opacity * texel;
        }`,
        blending: THREE.AdditiveBlending,
        depthTest: false,
        depthWrite: false,
        transparent: true
    });
    this.enabled = true;
    this.needsSwap = false;
    this.oldClearColor = new THREE.Color();
    this.oldClearAlpha = 1;
    this.basic = new THREE.MeshBasicMaterial();
    this.fsQuad = new THREE.FullScreenQuad(null);
};

THREE.UnrealBloomPass.prototype = {
    dispose: function () {
        for (var i = 0; i < this.renderTargetsHorizontal.length; i++) {
            this.renderTargetsHorizontal[i].dispose();
        }
        for (var i = 0; i < this.renderTargetsVertical.length; i++) {
            this.renderTargetsVertical[i].dispose();
        }
        this.renderTargetBright.dispose();
    },
    setSize: function (width, height) {
        this.renderTargetBright.setSize(width, height);
        for (var i = 0; i < this.nMips; i++) {
            this.renderTargetsHorizontal[i].setSize(width, height);
            this.renderTargetsVertical[i].setSize(width, height);
        }
    },
    render: function (renderer, writeBuffer, readBuffer, deltaTime, maskActive) {
        this.oldClearColor.copy(renderer.getClearColor());
        this.oldClearAlpha = renderer.getClearAlpha();
        renderer.setClearColor(this.clearColor, 0);
        if (maskActive) renderer.state.buffers.stencil.setTest(false);
        this.fsQuad.material = this.highPassUniforms;
        this.highPassUniforms["colorTexture"].value = readBuffer.texture;
        this.highPassUniforms["threshold"].value = this.threshold;
        this.fsQuad.render(renderer);
        var inputRenderTarget = this.renderTargetBright;
        for (var i = 0; i < this.nMips; i++) {
            this.fsQuad.material = this.separableBlurMaterials[i];
            this.separableBlurMaterials[i].uniforms["colorTexture"].value = inputRenderTarget.texture;
            this.separableBlurMaterials[i].uniforms["direction"].value = THREE.UnrealBloomPass.BlurDirectionX;
            renderer.setRenderTarget(this.renderTargetsHorizontal[i]);
            renderer.clear();
            this.fsQuad.render(renderer);
            this.separableBlurMaterials[i].uniforms["colorTexture"].value = this.renderTargetsHorizontal[i].texture;
            this.separableBlurMaterials[i].uniforms["direction"].value = THREE.UnrealBloomPass.BlurDirectionY;
            renderer.setRenderTarget(this.renderTargetsVertical[i]);
            renderer.clear();
            this.fsQuad.render(renderer);
            inputRenderTarget = this.renderTargetsVertical[i];
        }
        this.fsQuad.material = this.compositeMaterial;
        this.compositeMaterial.uniforms["bloomStrength"].value = this.strength;
        this.compositeMaterial.uniforms["bloomRadius"].value = this.radius;
        this.compositeMaterial.uniforms["bloomTintColors"].value = this.bloomTintColors;
        renderer.setRenderTarget(this.renderTargetsHorizontal[0]);
        renderer.clear();
        this.fsQuad.render(renderer);
        this.fsQuad.material = this.materialCopy;
        this.copyUniforms["tDiffuse"].value = this.renderTargetsHorizontal[0].texture;
        if (maskActive) renderer.state.buffers.stencil.setTest(true);
        if (this.renderToScreen) {
            renderer.setRenderTarget(null);
            this.fsQuad.render(renderer);
        } else {
            renderer.setRenderTarget(readBuffer);
            this.fsQuad.render(renderer);
        }
        renderer.setClearColor(this.oldClearColor, this.oldClearAlpha);
    },
    getSeperableBlurMaterial: function (kernelRadius) {
        return new THREE.ShaderMaterial({
            defines: {
                "KERNEL_RADIUS": kernelRadius,
                "SIGMA": kernelRadius
            },
            uniforms: {
                "colorTexture": { value: null },
                "invSize": { value: new THREE.Vector2(0.5, 0.5) },
                "direction": { value: new THREE.Vector2(0.5, 0.5) }
            },
            vertexShader: `varying vec2 vUv;
            void main() {
                vUv = uv;
                gl_Position = projectionMatrix * modelViewMatrix * vec4( position, 1.0 );
            }`,
            fragmentShader: `varying vec2 vUv;
            uniform sampler2D colorTexture;
            uniform vec2 invSize;
            uniform vec2 direction;
            void main() {
                vec2 invSize = vec2( 1.0 / textureSize( colorTexture, 0 ) );
                float fSigma = float( SIGMA );
                float weightSum = 0.0;
                vec3 colorSum = vec3( 0.0 );
                for ( int i = -KERNEL_RADIUS; i <= KERNEL_RADIUS; i ++ ) {
                    float x = float( i );
                    float w = exp( -x * x / ( 2.0 * fSigma * fSigma ) );
                    vec2 offset = direction * invSize * x;
                    vec3 sampleColor = texture2D( colorTexture, vUv + offset ).rgb;
                    colorSum += sampleColor * w;
                    weightSum += w;
                }
                gl_FragColor = vec4( colorSum / weightSum, 1.0 );
            }`
        });
    },
    getCompositeMaterial: function (nMips) {
        return new THREE.ShaderMaterial({
            defines: {
                "NUM_MIPS": nMips
            },
            uniforms: {
                "blurTexture1": { value: null },
                "blurTexture2": { value: null },
                "blurTexture3": { value: null },
                "blurTexture4": { value: null },
                "blurTexture5": { value: null },
                "bloomStrength": { value: 1.0 },
                "bloomFactors": { value: null },
                "bloomTintColors": { value: null },
                "bloomRadius": { value: 0.0 }
            },
            vertexShader: `varying vec2 vUv;
            void main() {
                vUv = uv;
                gl_Position = projectionMatrix * modelViewMatrix * vec4( position, 1.0 );
            }`,
            fragmentShader: `varying vec2 vUv;
            uniform sampler2D blurTexture1;
            uniform sampler2D blurTexture2;
            uniform sampler2D blurTexture3;
            uniform sampler2D blurTexture4;
            uniform sampler2D blurTexture5;
            uniform float bloomStrength;
            uniform float bloomRadius;
            uniform float bloomFactors[NUM_MIPS];
            uniform vec3 bloomTintColors[NUM_MIPS];
            void main() {
                vec4 color = vec4( 0.0 );
                color += texture2D( blurTexture1, vUv ) * bloomStrength * bloomFactors[0] * vec4( bloomTintColors[0], 1.0 );
                color += texture2D( blurTexture2, vUv ) * bloomStrength * bloomFactors[1] * vec4( bloomTintColors[1], 1.0 );
                color += texture2D( blurTexture3, vUv ) * bloomStrength * bloomFactors[2] * vec4( bloomTintColors[2], 1.0 );
                color += texture2D( blurTexture4, vUv ) * bloomStrength * bloomFactors[3] * vec4( bloomTintColors[3], 1.0 );
                color += texture2D( blurTexture5, vUv ) * bloomStrength * bloomFactors[4] * vec4( bloomTintColors[4], 1.0 );
                gl_FragColor = color;
            }`
        });
    }
};

THREE.UnrealBloomPass.BlurDirectionX = new THREE.Vector2(1.0, 0.0);
THREE.UnrealBloomPass.BlurDirectionY = new THREE.Vector2(0.0, 1.0);

// ====================== SHADER PASS ======================
THREE.ShaderPass = function (shader, textureID) {
    this.textureID = (textureID !== undefined) ? textureID : 'tDiffuse';
    if (shader instanceof THREE.ShaderMaterial) {
        this.uniforms = shader.uniforms;
        this.material = shader;
    } else if (shader) {
        this.uniforms = THREE.UniformsUtils.clone(shader.uniforms);
        this.material = new THREE.ShaderMaterial({
            defines: Object.assign({}, shader.defines),
            uniforms: this.uniforms,
            vertexShader: shader.vertexShader,
            fragmentShader: shader.fragmentShader
        });
    }
    this.renderToScreen = false;
    this.enabled = true;
    this.needsSwap = true;
    this.clear = false;
    this.fsQuad = new THREE.FullScreenQuad(this.material);
};

THREE.ShaderPass.prototype = {
    render: function (renderer, writeBuffer, readBuffer, deltaTime, maskActive) {
        if (this.uniforms[this.textureID]) {
            this.uniforms[this.textureID].value = readBuffer.texture;
        }
        this.fsQuad.material = this.material;
        if (this.renderToScreen) {
            renderer.setRenderTarget(null);
            this.fsQuad.render(renderer);
        } else {
            renderer.setRenderTarget(writeBuffer);
            if (this.clear) renderer.clear();
            this.fsQuad.render(renderer);
        }
    }
};

// ====================== FXAA SHADER ======================
THREE.FXAAShader = {
    uniforms: {
        "tDiffuse": { value: null },
        "resolution": { value: new THREE.Vector2(1 / 1024, 1 / 512) }
    },
    vertexShader: `varying vec2 vUv;
    void main() {
        vUv = uv;
        gl_Position = projectionMatrix * modelViewMatrix * vec4( position, 1.0 );
    }`,
    fragmentShader: `precision highp float;
    uniform sampler2D tDiffuse;
    uniform vec2 resolution;
    varying vec2 vUv;
    void main() {
        vec2 inverseVP = vec2( 1.0 / resolution.x, 1.0 / resolution.y );
        vec2 fragCoord = vUv * resolution;
        vec4 color = texture2D( tDiffuse, vUv );
        gl_FragColor = color;
    }`
};