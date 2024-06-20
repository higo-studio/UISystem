using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace Higo.UI
{
    [RequireComponent(typeof(Animator))]
    public class UIAnimation : MonoBehaviour, IUIPanelInit, IUIPanelShow, IUIPanelHide, IUIPanelResume, IUIPanelPause, IAnimationClipSource
    {
        [CreateAsset(typeof(AnimationClip), "anim")]
        public AnimationClip showAnimation;
        [CreateAsset(typeof(AnimationClip), "anim")]
        public AnimationClip hideAnimation;
        [CreateAsset(typeof(AnimationClip), "anim")]
        public AnimationClip resumeAnimation;
        [CreateAsset(typeof(AnimationClip), "anim")]
        public AnimationClip pauseAnimation;

        protected PanelInfo m_PanelInfo;
        protected PlayableGraph m_PlayableGraph;
        protected AnimationMixerPlayable m_Mixer;
        protected Animator m_Animator;
        protected bool m_DestroyRequestedAfterDone;

        protected void Awake()
        {
            m_Animator = GetComponent<Animator>();
            checkPlayable();
        }

        protected void OnDestroy()
        {
            if (m_PlayableGraph.IsValid())
            {
                m_PlayableGraph.Destroy();
            }
        }

        protected void Update()
        {
            if (m_DestroyRequestedAfterDone && m_PlayableGraph.IsValid() && m_PlayableGraph.IsDone())
            {
                m_DestroyRequestedAfterDone = false;
                UISystem.Instance.DestroyUI(m_PanelInfo.Uuid);
            }
        }

        protected void checkPlayable()
        {
            if (m_PlayableGraph.IsValid()) return;
            m_PlayableGraph = PlayableGraph.Create(nameof(UIAnimation));
            var show = AnimationClipPlayable.Create(m_PlayableGraph, showAnimation);
            var hide = AnimationClipPlayable.Create(m_PlayableGraph, hideAnimation);
            var resume = AnimationClipPlayable.Create(m_PlayableGraph, resumeAnimation);
            var pause = AnimationClipPlayable.Create(m_PlayableGraph, pauseAnimation);

            show.SetDuration(showAnimation?.averageDuration ?? 0);
            m_Mixer = AnimationMixerPlayable.Create(m_PlayableGraph, 4);
            m_PlayableGraph.Connect(show, 0, m_Mixer, 0);
            m_PlayableGraph.Connect(hide, 0, m_Mixer, 1);
            m_PlayableGraph.Connect(resume, 0, m_Mixer, 2);
            m_PlayableGraph.Connect(pause, 0, m_Mixer, 3);

            var output = AnimationPlayableOutput.Create(m_PlayableGraph, "Animator", m_Animator);
            PlayableOutputExtensions.SetSourcePlayable(output, m_Mixer);
        }

        public void OnInit(PanelInfo uuid)
        {
            this.m_PanelInfo = uuid;
        }

        private void selectClip(int index)
        {
            for (var i = 0; i < 4; i++)
            {
                m_Mixer.SetInputWeight(i, i == index ? 1 : 0);
            }
            var playableClip = (AnimationClipPlayable)m_Mixer.GetInput(index);
            var clip = playableClip.GetAnimationClip();
            playableClip.SetDuration(clip != null ? clip.length : float.MaxValue);
            m_Mixer.SetDuration(playableClip.GetDuration());
            m_Mixer.SetTime(0);
            m_Mixer.SetDone(false);
            playableClip.SetTime(0);
        }

        public void OnShow()
        {
            selectClip(0);
            m_PlayableGraph.Play();
        }

        public void OnHide(ref UIHideContext ctx)
        {
            ctx.DontDestroy = true;
            selectClip(1);
            m_PlayableGraph.Play();
            m_DestroyRequestedAfterDone = true;
        }

        public void OnResume()
        {
            selectClip(2);
            m_PlayableGraph.Play();
        }

        public void OnPause(ref UIPauseContext ctx)
        {
            selectClip(3);
            m_PlayableGraph.Play();
            m_DestroyRequestedAfterDone = true;
        }

        public void GetAnimationClips(List<AnimationClip> results)
        {
            if (showAnimation != null) results.Add(showAnimation);
            if (hideAnimation != null) results.Add(hideAnimation);
            if (resumeAnimation != null) results.Add(resumeAnimation);
            if (pauseAnimation != null) results.Add(pauseAnimation);
        }
    }
}