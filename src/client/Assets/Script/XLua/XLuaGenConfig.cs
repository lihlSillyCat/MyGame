/*
 * Tencent is pleased to support the open source community by making xLua available.
 * Copyright (C) 2016 THL A29 Limited, a Tencent company. All rights reserved.
 * Licensed under the MIT License (the "License"); you may not use this file except in compliance with the License. You may obtain a copy of the License at
 * http://opensource.org/licenses/MIT
 * Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing permissions and limitations under the License.
*/

using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using XLua;

//配置的详细介绍请看Doc下《XLua的配置.doc》
public static class XLuaGenConfig
{
    //lua中要使用到C#库的配置，比如C#标准库，或者Unity API，第三方库等。
    [LuaCallCSharp]
    public static List<Type> LuaCallCSharp = new List<Type>() {
                typeof(System.GC),
                typeof(System.Object),
                typeof(UnityEngine.Object),
                typeof(Vector2),
                typeof(Vector3),
                typeof(Vector4),
                typeof(Quaternion),
                typeof(Color),
                typeof(Ray),
                typeof(Bounds),
                typeof(Ray2D),
                typeof(Time),
                typeof(GameObject),
                typeof(Component),
                typeof(Behaviour),
                typeof(Transform),
                typeof(Resources),
                typeof(TextAsset),
                typeof(ShaderVariantCollection),
                typeof(Keyframe),
                typeof(AnimationCurve),
                typeof(AnimationClip),
                typeof(Animator),
                typeof(MonoBehaviour),
                typeof(ParticleSystem),
                typeof(SkinnedMeshRenderer),
                typeof(Renderer),
                typeof(LineRenderer),
                typeof(WWW),
                typeof(System.Collections.Generic.List<int>),
                typeof(Action<string>),
                typeof(Debug),
                typeof(Camera),
                typeof(PlayerPrefs),
                typeof(Mathf),
                typeof(Application),
                typeof(SystemLanguage),
                typeof(RaycastHit),
                typeof(RuntimePlatform),
                typeof(UnityEngine.Profiling.Profiler),
                typeof(UnityEngine.SceneManagement.SceneManager),

                typeof(Rigidbody),
                typeof(WheelCollider),
                typeof(Collision),
                typeof(Collider),
                typeof(SphereCollider),
                typeof(SystemInfo),
                typeof(Screen),
                typeof(QualitySettings),

                typeof(Input),
                typeof(KeyCode),

                // ugui
                typeof(UnityEngine.Events.UnityEvent),
                typeof(UnityEngine.Events.UnityEvent<Vector2>),
                //typeof(UnityEngine.UI.Events.UIEvent),
                //typeof(UnityEngine.UI.Events.UIEvent<float>),
                //typeof(UnityEngine.UI.Events.UIEvent<Vector2>),
                typeof(RectTransform),
                typeof(Button),
                typeof(Text),
                typeof(Image),
                typeof(Dropdown),
                typeof(InputField),
                typeof(Toggle),
                typeof(ScrollRect),
                typeof(DefaultControls),
                typeof(CanvasGroup),

                /*
                // custom
                typeof(War.Game.CharacterEntity),
                typeof(War.Game.LayerConfig),
                typeof(War.Game.PhysicsUtility),
                typeof(War.Game.ShadowRigid),
                typeof(War.Game.BodyPart),
                typeof(War.Game.WeaponGun),
                typeof(War.Game.GunSightEntity),
                typeof(War.Game.HitAction),
                typeof(War.Game.ChuteEntity),
                typeof(War.Game.DoorEntity),
                typeof(War.Game.TankEntity),
                typeof(War.Game.TankInfo),
                typeof(War.Game.TankBulletCollider),
                typeof(War.Game.TankHitPart),
                typeof(War.Game.TankHitAction),
                typeof(War.Game.Ani.EnActionID),
                typeof(War.Game.Ani.EnActionParamID),
                typeof(War.Game.FollowBonePosition),
                typeof(War.Game.MeleeColliderTrigger),
                typeof(War.Game.DoorPart),
                typeof(War.Game.ShellEntity),
                typeof(War.Game.AirplaneUpdater),

                typeof(Wheels),
                typeof(CarController),
                typeof(VehiclesSyncData),
                typeof(MotorboatController),
                typeof(BoatSyncData),

                typeof(War.Controller.CameraController),

                typeof(War.Scene.ObjectToMove),

                typeof(War.Render.ShaderCollectionWarmUp),

                typeof(UnityEngine.PostProcessing.PostProcessingBehaviour),
                typeof(UnityEngine.PostProcessing.PostProcessingProfile),
                typeof(UnityEngine.PostProcessing.PostProcessingModel),

                // custom ui
                typeof(ETCJoystick),
                typeof(War.UI.ClickButton),
                typeof(War.UI.SpriteAssets),
                typeof(War.UI.Draggable),
                typeof(War.UI.DraggableImage),
                typeof(War.UI.DropZone),
                typeof(War.UI.SpriteSwitcher),
                typeof(War.UI.Minimap),
                typeof(War.UI.MinimapLevel),
                typeof(War.UI.MinimapMarker),
                typeof(War.UI.MinimapInput),
                typeof(War.UI.MinimapZone),
                typeof(War.UI.MinimapDashLine),
                typeof(War.UI.DoubleClickButton),
                typeof(War.UI.PressButton),
                typeof(War.UI.SwipeOnScreen),
                typeof(War.UI.ClickedOnScreen),
                typeof(War.UI.Zoomer),
                typeof(War.UI.LocalizationText),
                typeof(War.UI.StateButton),
                typeof(War.UI.ClickedOnUI),
                typeof(War.UI.CScrollRect),
                typeof(War.UI.Drag),
                typeof(War.UI.InputHandler),
                typeof(War.UI.GyroscopeInput),
                typeof(War.UI.Team),
                */

                // facebook
                /*
                typeof(Facebook.Unity.AccessToken),
                typeof(Facebook.FacebookWrapper),
                typeof(Facebook.Unity.FB),
                typeof(Facebook.Unity.ILoginResult),
                typeof(Facebook.Unity.IShareResult),
                */

                // Fps
                //typeof(Utility.FPSCounter),

                //自动释放组件
                typeof(War.Base.GameObjectPool),
                 typeof(War.Base.RES_TYPE),
                typeof(War.Base.AutoReleaseToPool),
            };

    //C#静态调用Lua的配置（包括事件的原型），仅可以配delegate，interface
    [CSharpCallLua]
    public static List<Type> CSharpCallLua = new List<Type>() {
                typeof(Action),
                typeof(Func<double, double, double>),
                typeof(Action<string>),
                typeof(Action<double>),
                typeof(Action<UnityEngine.Object>),
                typeof(UnityEngine.Events.UnityAction),
                typeof(UnityEngine.Events.UnityAction<Vector2>),
                typeof(UnityEngine.Events.UnityAction<Boolean>),
                typeof(UnityEngine.Events.UnityAction<int>),
                typeof(UnityEngine.Events.UnityAction<float>),
                typeof(UnityEngine.Events.UnityAction<string>),

                typeof(System.Collections.IEnumerator),

                /*
                // custom
                typeof(War.Game.CharacterEntity.OnTakeWeaponEventHandler),
                typeof(War.Game.CharacterEntity.OnPutWeaponEventHandler),
                typeof(War.Game.CharacterEntity.OnThrowBombEventHandler),
                typeof(War.Game.CharacterEntity.OnJumpToProneEventHandler),
                typeof(War.Game.CharacterEntity.OnMeleeHitExitStateEventHandler),
                typeof(War.Game.CharacterEntity.OnMeleeHitAictionStateEventHandler),
                typeof(War.Game.CharacterEntity.OnEnterRigidityEventHandler),
                typeof(War.Game.CharacterEntity.OnLeaveRigidityEventHandler),
                typeof(War.Game.CharacterEntity.OnEnterFullRigidityEventHandler),
                typeof(War.Game.CharacterEntity.OnLeaveFullRigidityEventHandler),
                typeof(War.Game.CharacterEntity.OnLeftHandIKEventHandler),
                typeof(War.Game.CharacterEntity.OnRightHandIKEventHandler),
                typeof(War.Game.CharacterEntity.OnStartReloadEventHandler),
                typeof(War.Game.CharacterEntity.OnFinishReloadEventHandler),
                typeof(War.Game.CharacterEntity.OnExitReloadEventHandler),
                typeof(War.Game.CharacterEntity.OnStandStateEnterEventHandler),
                typeof(War.Game.CharacterEntity.OnCrouchStateEnterEventHandler),
                typeof(War.Game.CharacterEntity.OnProneStateEnterEventHandler),
                typeof(War.Game.CharacterEntity.OnWoundStateEnterEventHandler),
                typeof(War.Game.CharacterEntity.OnGunChamberingStateExitEventHandler),
                typeof(War.Game.CharacterEntity.OnWaterEventHandler),
                typeof(War.Game.CharacterEntity.OnPhysicsPropChange),
                typeof(War.Game.CharacterEntity.OnBeginSpecialJump),
                typeof(War.Game.CharacterEntity.OnEndSpecialJump),
                
                typeof(War.Game.CharacterEntity.OnMeleeHitEventHandler),
                typeof(War.Game.CharacterEntity.OnHitGroundEventHandler),
                typeof(War.Game.WeaponGun.OnRecoilBackEventHandler),
                typeof(War.Game.WeaponGun.OnRecoilRotateEventHandler),
                typeof(War.Game.WeaponGun.SpawnBulletEventHandler),
                typeof(War.Game.CharacterEntity.OnWaterEventHandler),
                typeof(War.Game.TankEntity.OnTankEventHandler),
                typeof(War.Game.DoorEntity.OnColliderHandler),
                typeof(War.Game.DoorEntity.OnDestroyHandler),
                typeof(War.Game.BoxEntity.OnColliderHandler),
                typeof(War.Game.BoxEntity.OnCollisionHandler),
                typeof(War.Game.ShellEntity.OnCollider),
                typeof(War.Game.MeleeColliderTrigger.OnMeleeHitEventHandle),
                typeof(War.Game.BombEntity.OnBombCollideEventHandle),
                typeof(War.Game.BombEntity.OnBombTriggerEventHandle),
                typeof(War.Controller.CameraController.OnCameraRotate),

                typeof(CarCollisionProcess),
                typeof(BoatCollisionProcess),

                // custom ui
                typeof(War.UI.Draggable.OnAcceptEventHandler),
                typeof(War.UI.Draggable.OnBeginDragEventHandler),
                typeof(War.UI.Draggable.OnEndDragEventHandler),
                typeof(War.UI.Draggable.GetUserDataEventHandler),
                typeof(War.UI.Draggable.GetDragEnableEventHandler),
                typeof(War.UI.DropZone.OnAcceptEventHandler),
                typeof(War.UI.Zoomer.OnZoomEventHandler),

                typeof(War.UI.LogMessageReceiver),
                typeof(War.UI.LogMessageReceiver.OnLogMessageReceive),
                typeof(War.UI.StateButton.ClickedEvent),
                typeof(War.UI.ClickedOnUI.OnMouseClickEventHandler),

                typeof(War.UI.CScrollRect.OnEndDragCall),
                typeof(War.UI.CScrollRect.OnDragCall),
                typeof(War.UI.CScrollRect.OnBeginDragCall),

                typeof(War.UI.Drag.OnBeginDragCall),
                typeof(War.UI.Drag.OnDragCall),
                typeof(War.UI.Drag.OnEndDragCall),

                typeof(War.UI.InputHandler.OnHandler),

                */

                // Facebook
                //typeof(Facebook.Unity.InitDelegate),
            };

    //黑名单
    [BlackList]
    public static List<List<string>> BlackList = new List<List<string>>()  {
                new List<string>(){"UnityEngine.WWW", "movie"},
                new List<string>(){"UnityEngine.Texture2D", "alphaIsTransparency"},
                new List<string>(){"UnityEngine.Security", "GetChainOfTrustValue"},
                new List<string>(){"UnityEngine.CanvasRenderer", "onRequestRebuild"},
                new List<string>(){"UnityEngine.Light", "areaSize"},
                new List<string>(){"UnityEngine.AnimatorOverrideController", "PerformOverrideClipListCleanup"},
    #if !UNITY_WEBPLAYER
                new List<string>(){"UnityEngine.Application", "ExternalEval"},
    #endif
                new List<string>(){"UnityEngine.GameObject", "networkView"}, //4.6.2 not support
                new List<string>(){"UnityEngine.Component", "networkView"},  //4.6.2 not support
                new List<string>(){"System.IO.FileInfo", "GetAccessControl", "System.Security.AccessControl.AccessControlSections"},
                new List<string>(){"System.IO.FileInfo", "SetAccessControl", "System.Security.AccessControl.FileSecurity"},
                new List<string>(){"System.IO.DirectoryInfo", "GetAccessControl", "System.Security.AccessControl.AccessControlSections"},
                new List<string>(){"System.IO.DirectoryInfo", "SetAccessControl", "System.Security.AccessControl.DirectorySecurity"},
                new List<string>(){"System.IO.DirectoryInfo", "CreateSubdirectory", "System.String", "System.Security.AccessControl.DirectorySecurity"},
                new List<string>(){"System.IO.DirectoryInfo", "Create", "System.Security.AccessControl.DirectorySecurity"},
                new List<string>(){"UnityEngine.MonoBehaviour", "runInEditMode"},

                new List<string>(){"UnityEngine.UI.Text", "OnRebuildRequested"},

                new List<string>(){ "UnityEngine.PostProcessing.PostProcessingProfile", "MonitorSettings"},
                new List<string>(){ "UnityEngine.PostProcessing.PostProcessingProfile", "monitors"},
            };
}
