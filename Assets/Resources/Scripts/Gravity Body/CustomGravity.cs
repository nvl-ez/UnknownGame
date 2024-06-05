using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Net.Mail;

namespace Proyect
{
    public static class CustomGravity
    {
        static List<GravitySource> sources = new List<GravitySource>();


        public static Vector3 GetGravity(Vector3 position) {
            Vector3 g = Vector3.zero;

            int maxPriority = sources[0].priority;

            for (int i = 0; i < sources.Count; i++) {
                if (sources[i].priority == maxPriority) { //If the sources have the maxPriority
                    g += sources[i].GetGravity(position);
                } else if (g.magnitude > 0 && sources[i].priority < maxPriority) { //If we're inside a main priority
                    break;
                } else { //If none of the max priorities added a magnitude
                    maxPriority = sources[i].priority;
                    g += sources[i].GetGravity(position);
                }
            }

            if (g.magnitude == 0) return -position.normalized*9.8f;
            return g;
        }

        public static Vector3 GetGravity(Vector3 position, out Vector3 upAxis) {
            Vector3 g = Vector3.zero;

            int maxPriority = sources[0].priority;

            for (int i = 0; i < sources.Count; i++) {
                if (sources[i].priority == maxPriority) { //If the sources have the maxPriority
                    g += sources[i].GetGravity(position);
                } else if (g.magnitude > 0 && sources[i].priority < maxPriority) { //If we're inside a main priority
                    break;
                } else { //If none of the max priorities added a magnitude
                    maxPriority = sources[i].priority;
                    g += sources[i].GetGravity(position);
                }
            }
            if (g.magnitude == 0) { 
                upAxis = position.normalized;
                return -position.normalized * 9.8f; 
            }

            upAxis = -g.normalized;
            return g;
        }

        public static Vector3 GetUpAxis(Vector3 position) {
            Vector3 g = Vector3.zero;

            int maxPriority = sources[0].priority;

            for (int i = 0; i < sources.Count; i++) {
                if (sources[i].priority == maxPriority) { //If the sources have the maxPriority
                    g += sources[i].GetGravity(position);
                } else if (g.magnitude > 0 && sources[i].priority < maxPriority) { //If we're inside a main priority
                    break;
                } else { //If none of the max priorities added a magnitude
                    maxPriority = sources[i].priority;
                    g += sources[i].GetGravity(position);
                }
            }
            return -g.normalized;
        }

        public static void Register(GravitySource source) {
            Debug.Assert(
                !sources.Contains(source),
                "Duplicate registration of gravity source!", source
            );
            sources.Add(source);
            sources = sources.OrderByDescending(gs => gs.priority).ToList();
        }

        public static void Unregister(GravitySource source) {
            Debug.Assert(
                sources.Contains(source),
                "Unregistration of unknown gravity source!", source
            );
            sources.Remove(source);
        }
    }
}
