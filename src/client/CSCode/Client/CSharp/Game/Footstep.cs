using UnityEngine;


namespace War.Game
{
    public class Footsteps : MonoBehaviour
    {
        [SerializeField]
        protected CharacterEntity m_CharacterEntity;

        #region Walk events
        [SerializeField]
        private float m_WalkEventThreshold = 0.4f;

        [SerializeField]
        protected AK.Wwise.Event m_FootStepsDashLeftEvent = null;
        [SerializeField]
        protected AK.Wwise.Event m_FootStepsDashRightEvent = null;

        [SerializeField]
        protected AK.Wwise.Event m_FootStepsProwlLeftEvent = null;
        [SerializeField]
        protected AK.Wwise.Event m_FootStepsProwlRightEvent = null;

        [SerializeField]
        protected AK.Wwise.Event m_FootStepsCrawlLeftEvent = null;
        [SerializeField]
        protected AK.Wwise.Event m_FootStepsCrawlRightEvent = null;

        [SerializeField]
        protected AK.Wwise.Event m_FootWalkLeftEvent = null;
        [SerializeField]
        protected AK.Wwise.Event m_FootWalkRightEvent = null;

        [SerializeField]
        protected AK.Wwise.Event m_FootRunLeftEvent = null;
        [SerializeField]
        protected AK.Wwise.Event m_FootRunRightEvent = null;

        [SerializeField]
        protected AK.Wwise.Event m_SwinLeftEvent = null;
        [SerializeField]
        protected AK.Wwise.Event m_SwinRightEvent = null;

        [SerializeField]
        protected AK.Wwise.Event m_SwinBothEvent = null;

        [SerializeField]
        protected AK.Wwise.Event m_SwinStandEvent = null;
        #endregion

        [SerializeField]
        protected float m_NormalEventThresold = 0.01f;

        [SerializeField]
        protected AK.Wwise.Event m_JumpUpEvent = null;

        [SerializeField]
        protected AK.Wwise.Event m_JumpDownEvent = null;

        [SerializeField]
        protected AK.Wwise.Event m_LieDownEvent = null;

        [SerializeField]
        protected AK.Wwise.Event m_LeftFistEvent = null;

        [SerializeField]
        protected AK.Wwise.Event m_MidFistEvent = null;

        [SerializeField]
        protected AK.Wwise.Event m_RightFistEvent = null;

        [SerializeField]
        protected AK.Wwise.Event m_CrouchEvent = null;
        
        void OnFootStepsDashLeft(AnimationEvent animationEvent)
        {
            if (!IsFootSoundValid())
            {
                return;
            }
            if (m_FootStepsDashLeftEvent != null && CheckAnimationThreshold(animationEvent, m_WalkEventThreshold))
            {
                m_FootStepsDashLeftEvent.Post(gameObject);
            }
        }

        void OnFootStepsDashRight(AnimationEvent animationEvent)
        {
            if (!IsFootSoundValid())
            {
                return;
            }
            if (m_FootStepsDashRightEvent != null && CheckAnimationThreshold(animationEvent, m_WalkEventThreshold))
            {
                m_FootStepsDashRightEvent.Post(gameObject);
            }
        }

        void OnFootStepsProwlLeft(AnimationEvent animationEvent)
        {
            if (!IsFootSoundValid())
            {
                return;
            }
            if (m_FootStepsProwlLeftEvent != null && CheckAnimationThreshold(animationEvent, m_WalkEventThreshold))
            {
                m_FootStepsProwlLeftEvent.Post(gameObject);
            }
        }

        void OnFootStepsProwlRight(AnimationEvent animationEvent)
        {
            if (!IsFootSoundValid())
            {
                return;
            }
            if (m_FootStepsProwlRightEvent != null && CheckAnimationThreshold(animationEvent, m_WalkEventThreshold))
            {
                m_FootStepsProwlRightEvent.Post(gameObject);
            }
        }

        void OnFootStepsCrawlLeft(AnimationEvent animationEvent)
        {
            if (!IsFootSoundValid())
            {
                return;
            }
            if (m_FootStepsCrawlLeftEvent != null && CheckAnimationThreshold(animationEvent, m_WalkEventThreshold))
            {
                m_FootStepsCrawlLeftEvent.Post(gameObject);
            }
        }

        void OnFootStepsCrawlRight(AnimationEvent animationEvent)
        {
            if (!IsFootSoundValid())
            {
                return;
            }
            if (m_FootStepsCrawlRightEvent != null && CheckAnimationThreshold(animationEvent, m_WalkEventThreshold))
            {
                m_FootStepsCrawlRightEvent.Post(gameObject);
            }
        }

        void OnFootWalkLeft(AnimationEvent animationEvent)
        {
            if (!IsFootSoundValid())
            {
                return;
            }
            if (m_FootWalkLeftEvent != null && CheckAnimationThreshold(animationEvent, m_WalkEventThreshold))
            {
                m_FootWalkLeftEvent.Post(gameObject);
            }
        }

        void OnFootWalkRight(AnimationEvent animationEvent)
        {
            if (!IsFootSoundValid())
            {
                return;
            }
            if (m_FootWalkRightEvent != null && CheckAnimationThreshold(animationEvent, m_WalkEventThreshold))
            {
                m_FootWalkRightEvent.Post(gameObject);
            }
        }

        void OnFootRunLeft(AnimationEvent animationEvent)
        {
            if (!IsFootSoundValid())
            {
                return;
            }
            if (m_FootRunLeftEvent != null && CheckAnimationThreshold(animationEvent, m_WalkEventThreshold))
            {
                m_FootRunLeftEvent.Post(gameObject);
            }
        }

        void OnFootRunRight(AnimationEvent animationEvent)
        {
            if (!IsFootSoundValid())
            {
                return;
            }
            if (m_FootRunRightEvent != null && CheckAnimationThreshold(animationEvent, m_WalkEventThreshold))
            {
                m_FootRunRightEvent.Post(gameObject);
            }
        }

        void OnSwinLeft(AnimationEvent animationEvent)
        {
            if (!IsSwimSoundValid())
            {
                return;
            }
            if (m_SwinLeftEvent != null && CheckAnimationThreshold(animationEvent, m_WalkEventThreshold))
            {
                m_SwinLeftEvent.Post(gameObject);
            }

            m_CharacterEntity.OnSwimLeft();
        }

        void OnSwinRight(AnimationEvent animationEvent)
        {
            if (!IsSwimSoundValid())
            {
                return;
            }
            if (m_SwinRightEvent != null && CheckAnimationThreshold(animationEvent, m_WalkEventThreshold))
            {
                m_SwinRightEvent.Post(gameObject);
            }
            m_CharacterEntity.OnSwimRight();
        }

        void OnSwinBoth(AnimationEvent animationEvent)
        {
            if (!IsSwimSoundValid())
            {
                return;
            }
            if (m_SwinBothEvent != null && CheckAnimationThreshold(animationEvent, m_WalkEventThreshold))
            {
                m_SwinBothEvent.Post(gameObject);
            }
            m_CharacterEntity.OnSwimBoth();
        }

        void OnSwinStand(AnimationEvent animationEvent)
        {
            if (!IsSwimSoundValid())
            {
                return;
            }
            if (m_SwinStandEvent != null && CheckAnimationThreshold(animationEvent, m_WalkEventThreshold))
            {
                m_SwinStandEvent.Post(gameObject);
            }
        }

        void OnJumpUp(AnimationEvent animationEvent)
        {
            if (m_JumpUpEvent != null && CheckAnimationThreshold(animationEvent, m_NormalEventThresold))
            {
                m_JumpUpEvent.Post(gameObject);
            }
        }

        void OnJumpDown(AnimationEvent animationEvent)
        {
            if (m_JumpDownEvent != null && CheckAnimationThreshold(animationEvent, m_NormalEventThresold))
            {
                m_JumpDownEvent.Post(gameObject);
            }
        }

        void OnLieDown(AnimationEvent animationEvent)
        {
            if (m_LieDownEvent != null && CheckAnimationThreshold(animationEvent, m_NormalEventThresold))
            {
                m_LieDownEvent.Post(gameObject);
            }
        }

        void OnCrouch(AnimationEvent animationEvent)
        {
            if (m_CrouchEvent != null && CheckAnimationThreshold(animationEvent, m_NormalEventThresold))
            {
                m_CrouchEvent.Post(gameObject);
            }
        }

        void OnLeftFist(AnimationEvent animationEvent)
        {
            if (m_LeftFistEvent != null && CheckAnimationThreshold(animationEvent, m_NormalEventThresold))
            {
                m_LeftFistEvent.Post(gameObject);
            }
        }

        void OnMidFist(AnimationEvent animationEvent)
        {
            if (m_MidFistEvent != null && CheckAnimationThreshold(animationEvent, m_NormalEventThresold))
            {
                m_MidFistEvent.Post(gameObject);
            }
        }

        void OnRightFist(AnimationEvent animationEvent)
        {
            if (m_RightFistEvent != null && CheckAnimationThreshold(animationEvent, m_NormalEventThresold))
            {
                m_RightFistEvent.Post(gameObject);
            }
        }

        private bool CheckAnimationThreshold(AnimationEvent animationEvent, float threshold)
        {
            return animationEvent.animatorClipInfo.weight >= threshold;
        }

        private bool IsFootSoundValid()
        {
            var postureState = m_CharacterEntity.postureState;
            if (postureState <= PostureState.Prone)
            {
                return true;
            }
            return false;
        }

        private bool IsSwimSoundValid()
        {
            var postureState = m_CharacterEntity.postureState;
            if (postureState == PostureState.Swim)
            {
                return true;
            }

            return false;
        }
    }
}