using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.UI;

namespace ScreenPocket
{
    public class Outline8 : Outline
    {
        protected Outline8()
        { }

        public override void ModifyMesh(VertexHelper vh)
        {
            if (!IsActive())
                return;

            //Outlineの処理を終わらせておく
            base.ModifyMesh(vh);

            var verts = ListPool<UIVertex>.Get();
            vh.GetUIVertexStream(verts);

            var outline4VertexCount = verts.Count;
            //本描画の頂点数を保持
            var baseCount = outline4VertexCount / 5;

            var neededCapacity = baseCount * 9;//9個分(本描画+8方向)のキャパを取得
            if (verts.Capacity < neededCapacity)
                verts.Capacity = neededCapacity;

            //ずらし幅を取得しておく
            var length = effectDistance.magnitude;

            //上
            var start = outline4VertexCount - baseCount;
            var end = verts.Count;
            ApplyShadowZeroAlloc(verts, effectColor, start, end, 0f, length);

            //右
            start = end;
            end = verts.Count;
            ApplyShadowZeroAlloc(verts, effectColor, start, end, length, 0f);

            //下
            start = end;
            end = verts.Count;
            ApplyShadowZeroAlloc(verts, effectColor, start, end, 0f, -length);

            //左
            start = end;
            end = verts.Count;
            ApplyShadowZeroAlloc(verts, effectColor, start, end, -length, 0f);

            vh.Clear();
            vh.AddUIVertexTriangleStream(verts);
            ListPool<UIVertex>.Release(verts);
        }
    }
}

