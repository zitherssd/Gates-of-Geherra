using System;
using System.Collections.Generic;

namespace Assets.Scripts.Battle.Actions.HitWindows
{
    public class HitWindowManager
    {
        private List<HitWindow> hitWindows;
        private Dictionary<(Actor.Actor enemy, int windowIndex), int> hitCounts = new();
        private Dictionary<(Actor.Actor player, int windowIndex), int> playerTriggerCounts = new();
        
        public HitWindowManager(List<HitWindow> windows)
        {
            hitWindows = windows;
        }
        
        /// <summary>
        /// Returns false if enemy already hit maxHitsPerEnemy times in this window.
        /// Returns true if:
        ///   - windowIndex is invalid
        ///   - window.maxHitsPerEnemy is 0 (unlimited)
        ///   - enemy hasn't reached hit limit yet
        /// </summary>
        public bool CanHit(Actor.Actor enemy, int windowIndex)
        {
            if (windowIndex < 0 || windowIndex >= hitWindows.Count)
                return false;
            
            var window = hitWindows[windowIndex];
            if (window.maxHitsPerEnemy == 0)
                return true;
            
            var key = (enemy, windowIndex);
            int count = hitCounts.TryGetValue(key, out var c) ? c : 0;
            return count < window.maxHitsPerEnemy;
        }
        
        /// <summary>
        /// Increment hit count for this enemy in this window.
        /// </summary>
        public void RecordHit(Actor.Actor enemy, int windowIndex)
        {
            var key = (enemy, windowIndex);
            hitCounts[key] = hitCounts.GetValueOrDefault(key) + 1;
        }

        /// <summary>
        /// Returns false if this player already triggered this window maxTriggersPerPlayer times.
        /// Returns true if:
        ///   - windowIndex is invalid
        ///   - window.maxTriggersPerPlayer is 0 (unlimited)
        ///   - player hasn't reached trigger limit yet
        /// </summary>
        public bool CanTriggerWindowForPlayer(Actor.Actor player, int windowIndex)
        {
            if (windowIndex < 0 || windowIndex >= hitWindows.Count)
                return false;

            var window = hitWindows[windowIndex];
            if (window.maxTriggersPerPlayer == 0)
                return true;

            var key = (player, windowIndex);
            int count = playerTriggerCounts.TryGetValue(key, out var c) ? c : 0;
            return count < window.maxTriggersPerPlayer;
        }

        /// <summary>
        /// Increment trigger count for this player in this window.
        /// </summary>
        public void RecordWindowTriggerForPlayer(Actor.Actor player, int windowIndex)
        {
            var key = (player, windowIndex);
            playerTriggerCounts[key] = playerTriggerCounts.GetValueOrDefault(key) + 1;
        }
    }
}
