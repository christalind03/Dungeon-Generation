using Code.Scripts.Attributes;
using UnityEngine;

namespace Code.Scripts.Dungeon
{
    public class DungeonModuleEntrance : MonoBehaviour
    {
        [Required, SerializeField] private Transform entrancePoint;
        
        [Header("Entrance Renderers")]
        [Required, SerializeField] private GameObject openState;
        [Required, SerializeField] private GameObject closedState;

        public Transform EntrancePoint => entrancePoint;
        
        private void Awake()
        {
            EnableEntrance(true);
        }

        public void EnableEntrance(bool isOpen)
        {
            openState.SetActive(isOpen);
            closedState.SetActive(!isOpen);
        }
    }
}