using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIVertexDebug : BaseMeshEffect
{

    public Mesh mesh = null;

    public override void ModifyMesh(VertexHelper vh)
    {
        Debug.Log(vh.currentVertCount);

        if (mesh == null)
        {
            mesh = new Mesh();
        }
        vh.FillMesh(mesh);
    }
}