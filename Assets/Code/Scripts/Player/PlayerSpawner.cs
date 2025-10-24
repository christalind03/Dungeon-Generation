using Code.Scripts.Attributes.Required;
using UnityEngine;

namespace Code.Scripts.Player
{
    /// <summary>
    /// Handles the spawning and initialization of the player character within the scene.
    /// </summary>
    public class PlayerSpawner : MonoBehaviour
    {
        [Required]
        [SerializeField]
        [Tooltip("The player prefab to instantiate when spawning the player.")]
        private GameObject playerPrefab;

        /// <summary>
        /// Instantiates the <see cref="playerPrefab"/> in world space.
        /// </summary>
        public void SpawnPlayer()
        {
            var spawnPoint = GameObject.Find("Spawn_Point");
            if (spawnPoint is not null)
            {
                Instantiate(playerPrefab, spawnPoint.transform.position, spawnPoint.transform.rotation);
            }
        }
    }
}