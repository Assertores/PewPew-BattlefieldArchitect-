using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PPBA
{
	public enum PawnAnimations : byte { IDLE, RUN, BUILD, DIE }

	public class PawnAnimationController : MonoBehaviour
	{
		[SerializeField] private Animator _animator;

		#region Monobehaviour
		void Start()
		{

		}

		void Update()
		{

		}

		private void OnValidate()
		{
			if(null == _animator)
				_animator = GetComponent<Animator>();
		}
		#endregion

		public void SetAnimatorBools(PawnAnimations animation)
		{
			switch(animation)
			{
				case PawnAnimations.IDLE:
					_animator.SetBool("IsRun", false);
					_animator.SetBool("IsBuild", false);
					_animator.SetBool("IsIdle", true);
					_animator.SetBool("IsDead", false);
					return;
				case PawnAnimations.RUN:
					_animator.SetBool("IsRun", true);
					_animator.SetBool("IsBuild", false);
					_animator.SetBool("IsIdle", false);
					_animator.SetBool("IsDead", false);
					return;
				case PawnAnimations.BUILD:
					_animator.SetBool("IsRun", false);
					_animator.SetBool("IsBuild", true);
					_animator.SetBool("IsIdle", false);
					_animator.SetBool("IsDead", false);
					return;
				case PawnAnimations.DIE:
					_animator.SetBool("IsRun", false);
					_animator.SetBool("IsBuild", false);
					_animator.SetBool("IsIdle", false);
					_animator.SetBool("IsDead", true);
					return;
				default:
					_animator.SetBool("IsRun", false);
					_animator.SetBool("IsBuild", false);
					_animator.SetBool("IsIdle", true);
					_animator.SetBool("IsDead", false);
					return;
			}
		}
	}
}