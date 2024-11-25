using UnityEngine.UIElements;
using UnityEditor;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Jobs;
using UnityEngine;

namespace ATKVoxelEngine
{
    [CustomEditor(typeof(NoiseProfile_SO))]
    public class NoiseProfileEditor : Editor
    {
        const int MAX_RESOLUTION = 1024;

        public VisualTreeAsset _inspectorXML;
        VisualElement _inspector;
        NoiseProfile_SO _target;
        VisualElement _NoisePreviewVE;

        int _previewResolution = 256;
        Texture2D _noiseTexture = null;

        public override VisualElement CreateInspectorGUI()
        {
            // Create a new VisualElement to be the root of our Inspector UI.
            _inspector = new VisualElement();
            _inspectorXML.CloneTree(_inspector);

            try
            {
                InitElements(target as NoiseProfile_SO);
            }
            catch
            {

            }

            if (_target != null)
            {
                Label name = _inspector.Q<Label>("Header");
                name.text = _target.name;

                _noiseTexture = new Texture2D(_previewResolution, _previewResolution);

                if (_NoisePreviewVE == null)
                    _NoisePreviewVE = _inspector.Q<VisualElement>("NoisePreview");

                ValueSliders();
                Octaves();
                Threshold();
                DimensionChanged();
                NoiseTypeChanged();
                PreviewResolution();

                _inspector.Q<Button>("RefreshPreview").clicked += PreviewNoise;

                PreviewNoise();
            }

            // Return the finished Inspector UI.
            return _inspector;
        }

        public void InitElements(NoiseProfile_SO profile)
        {
            _target = profile;
        }

        void ValueSliders()
        {
            _inspector.Q<Slider>("ScaleX").RegisterValueChangedCallback((evt) =>
            {
                PreviewNoise();
            });

            _inspector.Q<Slider>("ScaleZ").RegisterValueChangedCallback((evt) =>
            {
                PreviewNoise();
            });

            _inspector.Q<Slider>("Amplitude").RegisterValueChangedCallback((evt) =>
            {
                PreviewNoise();
            });

            _inspector.Q<Slider>("Frequency").RegisterValueChangedCallback((evt) =>
            {
                PreviewNoise();
            });

            _inspector.Q<Slider>("Lacunarity").RegisterValueChangedCallback((evt) =>
            {
                PreviewNoise();
            });

            _inspector.Q<Slider>("Persistence").RegisterValueChangedCallback((evt) =>
            {
                PreviewNoise();
            });

            _inspector.Q<Slider>("Rotation").RegisterValueChangedCallback((evt) =>
            {
                PreviewNoise();
            });

            _inspector.Q<SliderInt>("Multiplier").RegisterValueChangedCallback((evt) =>
            {
                PreviewNoise();
            });
        }

        void PreviewResolution()
        {
            SliderInt resolutionSlider = _inspector.Q<SliderInt>("ResSlider");
            resolutionSlider.value = _previewResolution;

            IntegerField resolutionField = _inspector.Q<IntegerField>("ResField");
            resolutionField.value = _previewResolution;

            // Resolution Slider
            resolutionSlider.RegisterValueChangedCallback((evt) =>
            {
                int newVal = Mathf.Clamp(evt.newValue, EngineSettings.WorldSettings.ChunkSize.x, MAX_RESOLUTION);
                if (evt.newValue % EngineSettings.WorldSettings.ChunkSize.x != 0)
                    newVal = Mathf.CeilToInt(evt.newValue / EngineSettings.WorldSettings.ChunkSize.x) * EngineSettings.WorldSettings.ChunkSize.x;

                resolutionField.value = newVal;
                resolutionSlider.value = newVal;
                _previewResolution = newVal;

                PreviewNoise();
            });

            // Resolution Field
            resolutionField.RegisterValueChangedCallback((evt) =>
            {
                int newVal = Mathf.Clamp(evt.newValue, EngineSettings.WorldSettings.ChunkSize.x, MAX_RESOLUTION);
                if (evt.newValue % EngineSettings.WorldSettings.ChunkSize.x != 0)
                    newVal = Mathf.CeilToInt(evt.newValue / EngineSettings.WorldSettings.ChunkSize.x) * EngineSettings.WorldSettings.ChunkSize.x;

                resolutionSlider.value = newVal;
                resolutionField.value = newVal;
                _previewResolution = newVal;

                PreviewNoise();
            });

        }

        void Octaves()
        {
            SliderInt octavesSlider = _inspector.Q<SliderInt>("Octaves");

            octavesSlider.RegisterValueChangedCallback((evt) =>
            {
                if (_target.Dimension == NoiseDimension.THREE_D)
                {
                    // make sure the octaves are divisible by 3
                    if (evt.newValue % 3 != 0)
                        octavesSlider.value = evt.newValue + 1;
                }
                else
                    PreviewNoise();
            });
        }

        void Threshold()
        {
            // toggle the threshold slider on and off based on the toggle
            Toggle useThreshold = _inspector.Q<Toggle>("UseThreshold");

            useThreshold.RegisterValueChangedCallback((evt) =>
            {
                _inspector.Q<GroupBox>("Threshold").style.display = evt.newValue ? DisplayStyle.Flex : DisplayStyle.None;
                PreviewNoise();
            });

            _inspector.Q<FloatField>("ThresholdX").RegisterValueChangedCallback((evt) =>
            {
                PreviewNoise();
            });

            _inspector.Q<FloatField>("ThresholdY").RegisterValueChangedCallback((evt) =>
            {
                PreviewNoise();
            });
        }

        void DimensionChanged()
        {
            // ScaleY
            EnumField dimensionsField = _inspector.Q<EnumField>("Dimensions");

            dimensionsField.RegisterValueChangedCallback((evt) =>
            {
                _inspector.Q<Slider>("ScaleY").style.display = (NoiseDimension)evt.newValue == NoiseDimension.TWO_D ? DisplayStyle.None : DisplayStyle.Flex;

                PreviewNoise();
            });
        }

        void NoiseTypeChanged()
        {
            EnumField noiseTypeField = _inspector.Q<EnumField>("NoiseType");

            noiseTypeField.RegisterValueChangedCallback((evt) =>
            {
                PreviewNoise();
            });
        }

        void PreviewNoise()
        {
            NativeArray<int> outputNoise = new NativeArray<int>(_previewResolution * _previewResolution, Allocator.TempJob);
            NativeArray<float> octaves = new NativeArray<float>(_target.Octaves * 2, Allocator.TempJob);

            new NoiseJob(_target, outputNoise, octaves, 69, new int3(_previewResolution, 0, _previewResolution), int3.zero).Run();

            if (_noiseTexture.width != _previewResolution || _noiseTexture.height != _previewResolution)
                _noiseTexture = new Texture2D(_previewResolution, _previewResolution);

            for (int x = 0; x < _previewResolution; x++)
            {
                for (int z = 0; z < _previewResolution; z++)
                {
                    int index = x + z * _previewResolution;

                    float value = outputNoise[index];

                    if (_target.UseThreshold)
                    {
                        if (value < _target.Threshold.x || value > _target.Threshold.y)
                            value = 0;
                        else
                            value = 1;
                    }
                    else
                    {
                        // ensure the value is scaled between 0 and 1 based on the Magnitude
                        value = (value / (float)_target.Multiplier);
                    }

                    _noiseTexture.SetPixel(x, z, new Color(value, value, value));
                }
            }

            _noiseTexture.filterMode = FilterMode.Point;
            _noiseTexture.Apply();

            _NoisePreviewVE.style.backgroundImage = new StyleBackground(_noiseTexture);

            outputNoise.Dispose();
            octaves.Dispose();
        }
    }
}