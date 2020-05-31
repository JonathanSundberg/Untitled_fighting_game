using System;
using System.Linq;
using Logic.Collision;
using UnityEditor;
using Unity.Mathematics;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Animation = Logic.Characters.Animation;

namespace Editor
{
    public class AnimationEditor : EditorWindow
    {
        private const string UXML_PATH = "Assets/UIElements/HitboxTimeline.uxml";

        private Animation _animation;
        private IntegerField _frameInput;
        private SliderInt _frameSlider;
        private IntegerField _durationInput;
        private Button _addKeyframe;
        private Button _removeKeyframe;
        private VisualElement _hitboxParent;
        private VisualElement _keyframeParent;
        private ObjectField _animationField;
        private Slider _scaleSlider;
        
        private VisualElement _contextMenu;
        private EnumField _typeDropdown;
        private IntegerField _xField;
        private IntegerField _yField;
        private IntegerField _wField;
        private IntegerField _hField;
        private Button _deleteHitbox;

        private VisualElement _previewRoot;
        private (int keyframe, int index) _selectedHitbox = (-1, -1);
        private Button _addHitbox;

        private int SelectedFrame => _frameInput.value;

        [MenuItem("Custom Tools/Open Animation Editor _%#E")]
        public static void ShowWindow()
        {
            var window = GetWindow<AnimationEditor>();
            window.titleContent = new GUIContent("Animation Editor");
            window.minSize = new float2(250, 50);
        }

        public void OnEnable()
        {
            AssetDatabase
                .LoadAssetAtPath<VisualTreeAsset>(UXML_PATH)
                .CloneTree(rootVisualElement);

            SetupMainElements();
            SetupContextMenuElements();
        }

        private void SetupMainElements()
        {
            _previewRoot = rootVisualElement.Q<VisualElement>("preview-root");
            
            _hitboxParent = rootVisualElement.Q<VisualElement>("hitbox-parent");
            _keyframeParent = rootVisualElement.Q<VisualElement>("keyframe-parent");

            _animationField = rootVisualElement.Q<ObjectField>("animation-field");
            
            _frameInput = rootVisualElement.Q<IntegerField>("frame-input");
            _durationInput = rootVisualElement.Q<IntegerField>("duration-input");
            
            _frameSlider = rootVisualElement.Q<SliderInt>("frame-slider");
            _scaleSlider = rootVisualElement.Q<Slider>("scale-slider");
            
            _addKeyframe = rootVisualElement.Q<Button>("add-keyframe");
            _removeKeyframe = rootVisualElement.Q<Button>("remove-keyframe");
            _addHitbox = rootVisualElement.Q<Button>("add-hitbox");

            _animationField.RegisterValueChangedCallback(evt =>
            {
                _animation = evt.newValue as Animation;
                if (_animation == null) return;

                _durationInput.value = _animation.Duration;
                RebuildKeyframeDots();
                SetActiveFrame(0);
            });

            _frameInput.RegisterValueChangedCallback(evt =>
            {
                SetActiveFrame(evt.newValue);
            });
            
            _frameSlider.RegisterValueChangedCallback(evt =>
            {
                SetActiveFrame(evt.newValue);
            });
            
            _durationInput.RegisterValueChangedCallback(evt =>
            {
                var newValue = math.max(1, evt.newValue);
                
                _durationInput.SetValueWithoutNotify(newValue);
                _frameSlider.highValue = newValue;
                _animation.Duration = newValue;

                if (_frameSlider.value > newValue)
                {
                    SetActiveFrame(newValue);
                }
                
                RebuildKeyframeDots();
            });
            
            _scaleSlider.RegisterValueChangedCallback(evt =>
            {
                SetActiveFrame(SelectedFrame);
            });

            _addKeyframe.clicked += () =>
            {
                _animation.Hitboxes.AddKeyframe(SelectedFrame);
                SetActiveFrame(SelectedFrame);
                RebuildKeyframeDots();
            };
            
            _removeKeyframe.clicked += () =>
            {
                _animation.Hitboxes.RemoveKeyframe(SelectedFrame);
                SetActiveFrame(SelectedFrame);
                RebuildKeyframeDots();
            };
            
            _addHitbox.clicked += () =>
            {
                _animation.Hitboxes.AddHitbox(SelectedFrame, new Hitbox
                {
                    Type = HitboxType.Attack,
                    Position = 0,
                    Size = 500
                });
                
                SetActiveFrame(SelectedFrame);
            };


        }
        
        private void SetupContextMenuElements()
        {
            _contextMenu = rootVisualElement.Q<VisualElement>("context-menu");
            _typeDropdown = rootVisualElement.Q<EnumField>("type-dropdown");
            _xField = rootVisualElement.Q<IntegerField>("x-field");
            _yField = rootVisualElement.Q<IntegerField>("y-field");
            _wField = rootVisualElement.Q<IntegerField>("w-field");
            _hField = rootVisualElement.Q<IntegerField>("h-field");
            _deleteHitbox = rootVisualElement.Q<Button>("delete-hitbox");
            
            rootVisualElement.RegisterCallback((MouseDownEvent e) =>
            {
                _contextMenu.style.display = DisplayStyle.None;
            });
            
            _previewRoot.RegisterCallback((MouseLeaveEvent e) =>
            {
                if ((e.pressedButtons & 0x1) != 0) return;
                if (_previewRoot.worldBound.Contains(e.mousePosition)) return;
                _contextMenu.style.display = DisplayStyle.None;
            });

            _typeDropdown.RegisterValueChangedCallback(e => UpdateSelectedHitbox());
            _xField.RegisterValueChangedCallback(e => UpdateSelectedHitbox());
            _yField.RegisterValueChangedCallback(e => UpdateSelectedHitbox());
            _wField.RegisterValueChangedCallback(e => UpdateSelectedHitbox());
            _hField.RegisterValueChangedCallback(e => UpdateSelectedHitbox());
            _deleteHitbox.clicked += DeleteSelectedHitbox;
        }
        
        private void UpdateSelectedHitbox()
        {
            var hitbox = new Hitbox
            {
                Type = (HitboxType) _typeDropdown.value,
                Position = new int2(_xField.value, _yField.value),
                Size = new int2(_wField.value, _hField.value)
            };
            
            _animation.Hitboxes.SetHitbox(_selectedHitbox.keyframe, _selectedHitbox.index, hitbox);
            SetActiveFrame(_frameInput.value);
        }

        private void DeleteSelectedHitbox()
        {
            _animation.Hitboxes.RemoveHitbox(_selectedHitbox.keyframe, _selectedHitbox.index);
            _contextMenu.style.display = DisplayStyle.None;
            
            SetActiveFrame(_frameInput.value);
        }

        
        private void SetActiveFrame(int frame)
        {
            frame = math.clamp(frame, 0, _animation.Duration);
            
            _frameInput.SetValueWithoutNotify(frame);
            _frameSlider.SetValueWithoutNotify(frame);

            var isKeyframe = _animation.Hitboxes.GetKeyframes().Contains(frame);
            
            _addKeyframe.style.opacity = isKeyframe ? 0.5f : 1f;
            _addKeyframe.pickingMode = isKeyframe ? PickingMode.Ignore : PickingMode.Position;
            
            _removeKeyframe.style.opacity = isKeyframe ? 1f : 0.5f;
            _removeKeyframe.pickingMode = isKeyframe ? PickingMode.Position : PickingMode.Ignore;

            _hitboxParent.Clear();

            var keyframeIndex = _animation.Hitboxes.GetKeyframeIndex(frame);
            for (var hitboxIndex = 0; hitboxIndex < _animation.Hitboxes[frame].Length; hitboxIndex++)
            {
                var hitbox = _animation.Hitboxes[frame][hitboxIndex];
                _hitboxParent.Add(CreateHitboxElement(keyframeIndex, hitboxIndex, hitbox));
            }
        }

        private VisualElement CreateHitboxElement(int keyframeIndex, int hitboxIndex, Hitbox hitbox)
        {
            var hitboxElement = new VisualElement();
            hitboxElement.AddToClassList("hitbox");
            hitboxElement.AddToClassList(hitbox.Type == HitboxType.Attack ? "hitbox-attack" : "hitbox-body");

            var x = hitbox.Position.x * _scaleSlider.value;
            var y = hitbox.Position.y * _scaleSlider.value;
            var w = hitbox.Size.x * _scaleSlider.value;
            var h = hitbox.Size.y * _scaleSlider.value;

            hitboxElement.style.left   = x - w * 0.5f;
            hitboxElement.style.bottom = y;
            hitboxElement.style.width  = w;
            hitboxElement.style.height = h;
            
            hitboxElement.RegisterCallback((MouseMoveEvent e) =>
            {
                if (!e.target.HasMouseCapture())
                {
                    var size = (float2) hitboxElement.layout.size;
                    var mousePosition = (float2) e.localMousePosition;

                    var topLeftOffset = mousePosition;
                    var topLeftNormalized = topLeftOffset / size;
                    var bottomRightOffset = size - mousePosition;
                    var bottomRightNormalized = bottomRightOffset / size ;
                    var centerOffset = math.abs(size / 2 - mousePosition);
                    var centerNormalized = centerOffset / size;

                    var t = topLeftOffset.y < 50 && topLeftNormalized.y < 0.2;
                    var l = topLeftOffset.x < 50 && topLeftNormalized.x < 0.2;
                    var b = bottomRightOffset.y < 50 && bottomRightNormalized.y < 0.2;
                    var r = bottomRightOffset.x < 50 && bottomRightNormalized.x < 0.2;

                    if (math.all(centerOffset < 25) && math.all(centerNormalized < 0.1f))
                    {
                        t = l = b = r = true;
                    }

                    hitboxElement.style.borderTopWidth    = t ? 3 : 1;
                    hitboxElement.style.borderLeftWidth   = l ? 3 : 1;
                    hitboxElement.style.borderBottomWidth = b ? 3 : 1;
                    hitboxElement.style.borderRightWidth  = r ? 3 : 1;
                }
                else
                {
                    var sizeDelta = new float2();
                    var positionDelta = new float2();
                    
                    if (hitboxElement.style.borderTopWidth.value > 1f)
                    {
                        sizeDelta.y -= e.mouseDelta.y;
                    } 
                    if (hitboxElement.style.borderRightWidth.value > 1f) 
                    {
                        sizeDelta.x += e.mouseDelta.x;
                    }
                    if (hitboxElement.style.borderBottomWidth.value > 1f) 
                    {
                        sizeDelta.y += e.mouseDelta.y;
                        positionDelta.y -= e.mouseDelta.y;
                    } 
                    if (hitboxElement.style.borderLeftWidth.value > 1f) 
                    {
                        sizeDelta.x -= e.mouseDelta.x;
                        positionDelta.x += e.mouseDelta.x;
                    }

                    hitboxElement.style.width  = hitboxElement.style.width.value.value  + sizeDelta.x;
                    hitboxElement.style.height = hitboxElement.style.height.value.value + sizeDelta.y;
                    hitboxElement.style.left   = hitboxElement.style.left.value.value   + positionDelta.x;
                    hitboxElement.style.bottom = hitboxElement.style.bottom.value.value + positionDelta.y;
                }
            });
            
            hitboxElement.RegisterCallback((MouseDownEvent e) =>
            {
                if ((e.pressedButtons & 0x1) != 0)
                {
                    e.target.CaptureMouse();
                }
                else if ((e.pressedButtons & 0x2) != 0)
                {
                    e.StopImmediatePropagation();
                    ShowContextMenu
                    (
                        e.mousePosition - _previewRoot.worldBound.position,
                        keyframeIndex,
                        hitboxIndex,
                        hitbox
                    );
                }
            });
            
        
            hitboxElement.RegisterCallback((MouseUpEvent e) =>
            {
                if (e.target.HasMouseCapture())
                {
                    e.target.ReleaseMouse();

                    var size = new float2
                    (
                        hitboxElement.style.width.value.value,
                        hitboxElement.style.height.value.value
                    );
                    var position = new float2
                    (
                        hitboxElement.style.left.value.value + size.x / 2,
                        hitboxElement.style.bottom.value.value
                    );

                    var scale = 1 / _scaleSlider.value;
                    hitbox.Size = (int2) (size * scale);
                    hitbox.Position = (int2) (position * scale);

                    _animation.Hitboxes.SetHitbox(keyframeIndex, hitboxIndex, hitbox);

                    var mousePosition = (float2) e.localMousePosition;
                    if (math.all(mousePosition < 0) 
                     || math.all(mousePosition > hitboxElement.layout.size))
                    {
                        hitboxElement.style.borderLeftWidth = 1;
                        hitboxElement.style.borderRightWidth = 1;
                        hitboxElement.style.borderTopWidth = 1;
                        hitboxElement.style.borderBottomWidth = 1;
                    }
                }
            });

            hitboxElement.RegisterCallback((MouseOutEvent e) =>
            {
                if (e.target.HasMouseCapture()) return;
                hitboxElement.style.borderLeftWidth   = 1;
                hitboxElement.style.borderRightWidth  = 1;
                hitboxElement.style.borderTopWidth    = 1;
                hitboxElement.style.borderBottomWidth = 1;
            });
            
            hitboxElement.RegisterCallback((WheelEvent e) =>
            {
                if (e.delta.y < 0)
                {
                    hitboxElement.SendToBack();
                }
                else if (e.delta.y > 0)
                {
                    hitboxElement.BringToFront();
                }
            });

            return hitboxElement;
        }

        private void ShowContextMenu(float2 menuPosition, int keyframe, int index, Hitbox hitbox)
        {
            _typeDropdown.SetValueWithoutNotify(hitbox.Type);
            _xField.SetValueWithoutNotify(hitbox.Position.x);
            _yField.SetValueWithoutNotify(hitbox.Position.y);
            _wField.SetValueWithoutNotify(hitbox.Size.x);
            _hField.SetValueWithoutNotify(hitbox.Size.y);

            _contextMenu.style.display = DisplayStyle.Flex;
            _contextMenu.style.left = menuPosition.x;
            _contextMenu.style.top = menuPosition.y;
            
            _selectedHitbox = (keyframe, index);
        }

        private void RebuildKeyframeDots()
        {
            _keyframeParent.Clear();

            foreach (var keyframe in _animation.Hitboxes.GetKeyframes())
            {
                var dot = new VisualElement();
                dot.AddToClassList("keyframe-dot");

                var percentOffset = (float) keyframe / _animation.Duration;
                dot.style.left = new Length(percentOffset * 100, LengthUnit.Percent);

                _keyframeParent.Add(dot);
            }
        }
    }
}
