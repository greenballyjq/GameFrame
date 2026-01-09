using UnityEngine;
using UnityEngine.UI;

namespace GameFramework.Common.UI
{
    // public class UIGameTeam : UIBase
    public class UIGameTeam : MonoBehaviour
    {
        [SerializeField] private Button confirmButton;
        [SerializeField] private Animator animator;

        // public override void OnInit(object userData)
        // {
        //     confirmButton.onClick.AddListener(() =>
        //     {
        //         Close();
        //     });
        //     base.OnInit(userData);
        // }
        private void Start() 
        {
            confirmButton.onClick.AddListener(() =>
            {
                gameObject.SetActive(false);
            });
        }
    }
}