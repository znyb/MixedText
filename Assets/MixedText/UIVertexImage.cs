using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIVertexImage : MaskableGraphic
{
    private Texture myTexture;
    List<UIVertex> myVertices;
    readonly UIVertex[] myTempVerts = new UIVertex[4];

    public override Texture mainTexture
    {
        get
        {
            if(MyTexture)
            {
                return MyTexture;
            }
            return base.mainTexture;
        }
    }

    public List<UIVertex> MyVertices
    {
        get
        {
            return myVertices;
        }

        set
        {
            myVertices = value;
            //LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);
            //SetVerticesDirty();
        }
    }

    public Texture MyTexture
    {
        get
        {
            return myTexture;
        }

        set
        {
            myTexture = value;
            SetMaterialDirty();
        }
    }

    public UIVertexImage()
    {
        useLegacyMeshGeneration = false;
    }

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();
        if (MyVertices != null)
        {
            int vertCount = MyVertices.Count;
            for (int i = vertCount - 1; i >= 0; --i)
            {
                int tempVertsIndex = 3 - i & 3;
                UIVertex vertex = MyVertices[i];
                vertex.color = color;
                myTempVerts[tempVertsIndex] = vertex;
                if (tempVertsIndex == 3)
                    vh.AddUIVertexQuad(myTempVerts);
            }
        }
    }
}

