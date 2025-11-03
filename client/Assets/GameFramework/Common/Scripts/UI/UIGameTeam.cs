using GameFramework.Core;
using UnityEngine;
using UnityEngine.UI;

namespace GameFramework.Common.UI
{
    public class UIGameTeam : UIBase
    {
        [SerializeField] private Button confirmButton;
        [SerializeField] private Animator animator;

        public override void OnInit(object userData)
        {
            confirmButton.onClick.AddListener(() =>
            {
                Close();
            });
            base.OnInit(userData);
        }
    }
}