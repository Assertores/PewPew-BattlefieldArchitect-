using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PPBA
{
	public enum PawnAnimations { IDLE, RUN, BUILD, DIE }

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
			_animator.SetBool("IsRun", false);
			_animator.SetBool("IsBuild", false);
			_animator.SetBool("IsIdle", false);

			switch(animation)
			{
				case PawnAnimations.IDLE:
					_animator.SetBool("IsRun", false);
					_animator.SetBool("IsBuild", false);
					_animator.SetBool("IsIdle", true);
					break;
				case PawnAnimations.RUN:
					_animator.SetBool("IsRun", true);
					_animator.SetBool("IsBuild", false);
					_animator.SetBool("IsIdle", false);
					break;
				case PawnAnimations.BUILD:
					_animator.SetBool("IsRun", false);
					_animator.SetBool("IsBuild", true);
					_animator.SetBool("IsIdle", false);
					break;
				case PawnAnimations.DIE:
					_animator.SetBool("IsRun", false);
					_animator.SetBool("IsBuild", false);
					_animator.SetBool("IsIdle", false);
					_animator.SetBool("IsDead", true);
					break;
				default:
					_animator.SetBool("IsRun", false);
					_animator.SetBool("IsBuild", false);
					_animator.SetBool("IsIdle", true);
					break;
			}
		}
	}
}