using Code.Scripts.Attributes;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Code.Scripts.Dungeon
{
    public class DungeonModule : MonoBehaviour
    {
        [Required, SerializeField] private Collider[] moduleBounds;
        [Required, SerializeField] private DungeonModuleEntrance[] moduleEntrances;
        
        private List<DungeonModuleEntrance> availableEntrances;

        private void Awake()
        {
            availableEntrances = moduleEntrances.ToList();
        }
        
        public Bounds CalculateBounds()
        {
            var allBounds = moduleBounds.Select(moduleBound => moduleBound.bounds).ToArray();
            
            // Calculate the center of the bounding region
            var objectCenter = allBounds.Aggregate(Vector3.zero, (current, currentBound) => current + currentBound.center);
            objectCenter /= allBounds.Length;
            
            // Calculate the boundaries of the object
            var objectBounds = new Bounds(objectCenter, Vector3.zero);

            foreach (var currentBounds in allBounds)
            {
                objectBounds.Encapsulate(currentBounds);
            }
            
            return objectBounds;
        }
        
        public void RemoveAvailableEntrance(Transform entrancePoint)
        {
            availableEntrances.RemoveAll(availableEntrance => availableEntrance.EntrancePoint == entrancePoint);
        }
        
        public Transform SelectAvailableEntrance()
        {
            if (availableEntrances.Count <= 0) return null;
            
            var entranceIndex = Random.Range(0, availableEntrances.Count);
            return availableEntrances[entranceIndex].EntrancePoint;
        }
        
        #if UNITY_EDITOR

        private void OnValidate()
        {
            var scriptName = GetType().Name;
            
            var missingBounds = false;
            var missingEntrances = false;
            
            if (moduleBounds is not { Length: > 0 })
            {
                Debug.LogError($"[{scriptName}] <b>{nameof(moduleBounds)}</b> must contain at least one element.", this);
                missingBounds = true;
            }

            if (moduleEntrances is not { Length: > 0 })
            {
                Debug.LogError($"[{scriptName}] <b>{nameof(moduleEntrances)}</b> must contain at least one element.", this);
                missingEntrances = true;
            }

            if ((missingBounds || missingEntrances) && EditorApplication.isPlaying)
            {
                EditorApplication.ExitPlaymode();
            }
        }
                
        #endif
    }
}