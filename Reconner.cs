﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Gist;
using Recon.SpacePartition;
using Recon.VisibleArea;
using Recon.BoundingVolumes;

namespace Recon {
    [ExecuteInEditMode]
    public class Reconner : MonoBehaviour {
        void OnEnable() {
            Instance = this;
        }
        void Update() {
            RebuildBVH ();
        }
        void OnDrawGizmos() {
            if (Reconner._bvh == null)
                return;
            Gizmos.color = gizmoColorBounds;
            Reconner._bvh.DrawBounds (0, 10);
        }

		public Color gizmoColorBounds = Color.green;
        public BVHController<Volume> BVH { get { return Reconner._bvh; } }

        public BVHController<Volume> RebuildBVH () {
            Reconner._bounds.Clear ();
            var vals = Reconner._database.GetList ();
            for (var i = 0; i < vals.Count; i++)
                Reconner._bounds.Add (vals [i].GetBounds ());
            return Reconner._bvh.Build (Reconner._bounds, vals);
        }

        #region Static
		static Dataset<Volume> _database;
		static BVHController<Volume> _bvh;
		static List<Bounds> _bounds;

		static Reconner() {
			_database = new Dataset<Volume> ();
			_bvh = new BVHController<Volume> ();
			_bounds = new List<Bounds> ();
		}

        public static Reconner Instance { get; protected set; }
        public static void Add(Volume vol) {
            _database.Add (vol);
        }
        public static bool Remove(Volume vol) {
            return _database.Remove (vol) >= 0;
        }

        #region Intersection
        public static readonly System.Func<Volume, bool> Pass = v => true;
        public static IEnumerable<Volume> Find(IConvexPolyhedron conv) { return Find(conv, Pass); }
        public static IEnumerable<Volume> Find(IConvexPolyhedron conv, System.Func<Volume, bool> Filter) {
            var r = Instance;
            BVHController<Volume> bvh;
            if (r == null || (bvh = r.BVH) == null)
                yield break;
            
            var bb = conv.WorldBounds ();
            foreach (var v in NarrowPhase(conv, Broadphase(bvh, bb)))
                if (Filter(v))
                    yield return v;                
        }
        public static IEnumerable<Volume> Broadphase(BVHController<Volume> bvh, Bounds bb) {
            foreach (var v in bvh.Intersect(bb))
                yield return v;
        }
        public static IEnumerable<Volume> NarrowPhase(IConvexPolyhedron conv, IEnumerable<Volume> broadphased) {
            foreach (var v in broadphased)
                if (v.GetConvexPolyhedron().Intersect(conv))
                    yield return v;
        }
        #endregion
        #endregion

    }
}
