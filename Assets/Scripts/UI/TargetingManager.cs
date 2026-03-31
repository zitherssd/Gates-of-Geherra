//using Assets.Scripts.Battle.Actions.Skills;
//using Assets.Scripts.Save;
//using Assets.Scripts.Utility;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using UnityEngine;
//using static Assets.Scripts.Battle.Actions.BaseAction;
//using UnityEngine.EventSystems;
//using Assets.Scripts.Battle.Actions;

//namespace Assets.Scripts.UI
//{
//    public class TargetingManager : MonoBehaviour
//    {
//        public static TargetingManager instance;
        
//        private Transform home;

//        private static float maxJoystickDistance = 100f;
//        private static RectTransform joystickBase;
//        private static RectTransform joystickKnob;
//        private static GameObject guide;

//        private Vector2 targetPos;
//        private static Vector2 joystickCenter;
//        void Awake()
//        {
//            instance = this;
//        }

//        public void BeingTargetingF(BaseAction referencedAction)
//        {

//            if (referencedAction is AttackSkill)
//            {
//                CameraManager.instance.SlowTrack = false;
//            }

//            switch (referencedAction.Type)
//            {
//                case BUTTONTYPE.INSTANT:
//                    //UseSkill nothing else
//                    break;
//                case BUTTONTYPE.VECTOR:
//                    CameraManager.instance.SlowTrack = false;
//                    UIManager.instance.HideAllButThis(referencedAction);
//                    UIManager.instance.GainMeter(referencedAction.SlowDownMeterGainOnPress);
//                    EnableJoystick(true);
//                    guide.GetComponent<ParticleSystem>().Play();

//                    isPressed = true;
//                    pointerDownPosition = Input.touchCount > 0 ? Input.GetTouch(0).position : (Vector2)Input.mousePosition;
//                    joystickBase.position = pointerDownPosition;
//                    break;
//                case BUTTONTYPE.CONTINNUOUS:
//                    HideUIAndUseAction();
//                    break;
//                case BUTTONTYPE.CONTINUOUS_VECTOR:
//                    deltaScaled = Vector2.zero;
//                    referencedAction.Direction = Vector3.zero;
//                    HideUIExceptThisAndUseAction();
//                    isPressed = true;
//                    pointerDownPosition = Input.touchCount > 0 ? Input.GetTouch(0).position : (Vector2)Input.mousePosition;
//                    EnableJoystick(true);
//                    //guide.GetComponent<ParticleSystem>().Play();
//                    referencedAction.OnCancel += DisableJoystick;

//                    joystickBase.position = pointerDownPosition;
//                    break;
//                default:
//                    break;
//            }

//            //Output the name of the GameObject that is being clicked
//        }
//    }
//}
