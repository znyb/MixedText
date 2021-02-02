using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class MixedText : Text 
{
    class ImageInfo
    {
        public string name;
        //0：自动适应字体大小
        //-1：原图大小
        //其他：图片高度=size，宽度等比缩放
        public int size;

        public void Reset()
        {
            name = null;
            size = 0;
        }
    }

    public UIVertexImage myImage;
    [SerializeField]
    private AtlasData myAtlasData;

    private static readonly string myQuadFormat = "<quad size={0} x={1} y={2} width={3} height={4} />";
    private static readonly Regex smyRegex = new Regex(@"<quad [^/>]*/>");
    private List<int> myImageVertexIndex = new List<int>();

    List<UIVertex> myImageVertexs = new List<UIVertex>();

    readonly UIVertex[] myTempVerts = new UIVertex[4];
    StringBuilder myStringBuilder = new StringBuilder();
    StringBuilder myImageNameBuilder = new StringBuilder();
    bool isTextDirty = false;
    string myRealText;
    //quad标签默认图片是宽高相等的，当图片宽高不等时会把图片拉伸成相等
    //使用这个图片宽高比例系数调节quad标签生成范围及生成后的图片顶点uv
    float myAtlasAspect = 1f;
    //DrivenRectTransformTracker imageTracker = new DrivenRectTransformTracker();
    ImageInfo imageInfo = new ImageInfo();

    public override string text
    {
        get
        {
            return m_Text;
        }

        set
        {
            if (string.IsNullOrEmpty(value))
            {
                if (string.IsNullOrEmpty(m_Text))
                    return;
                m_Text = "";
                SetVerticesDirty();
                isTextDirty = true;
            }
            else if (m_Text != value)
            {
                m_Text = value;
                SetVerticesDirty();
                SetLayoutDirty();
                isTextDirty = true;
            }
        }
    }

    public string realText
    {
        get
        {
            if(isTextDirty)
            {
                myRealText = Format(text);
                isTextDirty = false;
            }
            return myRealText;
        }
    }

    public AtlasData atlasData
    {
        get
        {
            return myAtlasData;
        }

        set
        {
            myAtlasData = value;
            UpdateAspect();
        }
    }

    public float MyAtlasAspect
    {
        get
        {
            return myAtlasAspect;
        }

        set
        {
            myAtlasAspect = value;
        }
    }

    void UpdateAspect()
    {
        if (myAtlasData != null && myAtlasData.myAtlas != null)
        {
            MyAtlasAspect = (float)myAtlasData.myAtlasWidth / myAtlasData.myAtlasHeight;
        }
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        if (myImage && atlasData)
        {
            myImage.MyTexture = atlasData.myAtlas;
        }
        UpdateAspect();

        isTextDirty = true;
    }

    public override void SetVerticesDirty()
    {
        base.SetVerticesDirty();
        if(myImage != null)
        {
            //CanvasUpdateRegistry.UnRegisterCanvasElementForRebuild(myImage);
            myImage.SetVerticesDirty();
            //imageTracker.Clear();
        }
        //StartCoroutine(DelaySetImageDirty());
    }

    //copy from Text
    protected override void OnPopulateMesh(VertexHelper toFill)
    {
        if (font == null)
            return;

        // We don't care if we the font Texture changes while we are doing our Update.
        // The end result of cachedTextGenerator will be valid for this instance.
        // Otherwise we can get issues like Case 619238.
        m_DisableFontTextureRebuiltCallback = true;

        Vector2 extents = rectTransform.rect.size;

        var settings = GetGenerationSettings(extents);
        cachedTextGenerator.PopulateWithErrors(realText, settings, gameObject);

        // Apply the offset to the vertices
        List<UIVertex> verts = cachedTextGenerator.verts as List<UIVertex>;
        float unitsPerPixel = 1 / pixelsPerUnit;
        //Last 4 verts are always a new line... (\n)
        int vertCount = verts.Count - 4;
        int characterCountVisible = cachedTextGenerator.characterCountVisible;
        //Debug.Log(verts);

        //分离图片顶点
        bool imageVertexChanged = false;
        if (vertCount >= characterCountVisible * 4)
        {
            UpdateImagesVertexIndex(realText, characterCountVisible);
            myImageVertexs.Clear();
            for (int i = myImageVertexIndex.Count - 1; i >= 0; i -= 2)
            {
                int startIndex = myImageVertexIndex[i - 1] * 4;
                int endIndex = myImageVertexIndex[i] * 4;

                //startIndex开始的前四个顶点为图片顶点，之后到endIndex的顶点为富文本标签字符生成的重复顶点
                //顶点顺序
                // 1 2
                // 4 3
                UIVertex vertex1 = verts[startIndex];
                UIVertex vertex2 = verts[startIndex + 1];
                UIVertex vertex3 = verts[startIndex + 2];
                UIVertex vertex4 = verts[startIndex + 3];

                //图片下移，quad标签默认生成的图片位置偏上
                float imageHeight = vertex1.position.y - vertex4.position.y;
                float offset = -imageHeight * 0.15f;

                vertex1.position.y += offset;
                vertex2.position.y += offset;
                vertex3.position.y += offset;
                vertex4.position.y += offset;

                //矫正图片顶点uv
                vertex2.uv0.x = vertex1.uv0.x + (vertex2.uv0.x - vertex1.uv0.x) / MyAtlasAspect;
                vertex3.uv0.x = vertex4.uv0.x + (vertex3.uv0.x - vertex4.uv0.x) / MyAtlasAspect;

                myImageVertexs.Add(vertex4);
                myImageVertexs.Add(vertex3);
                myImageVertexs.Add(vertex2);
                myImageVertexs.Add(vertex1);

                verts.RemoveRange(startIndex, endIndex - startIndex);
            }
            vertCount = verts.Count - 4;
            imageVertexChanged = true;
            //if(myImage != null)
            //{
            //    imageTracker.Add(this, myImage.rectTransform, DrivenTransformProperties.All);
            //}
        }
        
        Vector2 roundingOffset = new Vector2(verts[0].position.x, verts[0].position.y) * unitsPerPixel;
        roundingOffset = PixelAdjustPoint(roundingOffset) - roundingOffset;
        toFill.Clear();

        if (roundingOffset != Vector2.zero)
        {
            for (int i = 0; i < vertCount; ++i)
            {
                int tempVertsIndex = i & 3;
                myTempVerts[tempVertsIndex] = verts[i];
                myTempVerts[tempVertsIndex].position *= unitsPerPixel;
                myTempVerts[tempVertsIndex].position.x += roundingOffset.x;
                myTempVerts[tempVertsIndex].position.y += roundingOffset.y;
                if (tempVertsIndex == 3)
                    toFill.AddUIVertexQuad(myTempVerts);
            }
            if(imageVertexChanged)
            {
                int imageVertexCount = myImageVertexs.Count;
                for(int i = 0; i< imageVertexCount;i++)
                {
                    UIVertex vertex = myImageVertexs[i];
                    vertex.position *= unitsPerPixel;
                    vertex.position.x += roundingOffset.x;
                    vertex.position.y += roundingOffset.y;
                    myImageVertexs[i] = vertex;
                }
                if (myImage)
                {
                    myImage.MyVertices = myImageVertexs;
                }
            }
        }
        else
        {
            for (int i = 0; i < vertCount; ++i)
            {
                int tempVertsIndex = i & 3;
                myTempVerts[tempVertsIndex] = verts[i];
                myTempVerts[tempVertsIndex].position *= unitsPerPixel;
                if (tempVertsIndex == 3)
                    toFill.AddUIVertexQuad(myTempVerts);
            }
            if (imageVertexChanged)
            {
                int imageVertexCount = myImageVertexs.Count;
                for (int i = 0; i < imageVertexCount; i++)
                {
                    UIVertex vertex = myImageVertexs[i];
                    vertex.position *= unitsPerPixel;
                    myImageVertexs[i] = vertex;
                }
                if (myImage)
                {
                    myImage.MyVertices = myImageVertexs;
                }
            }
        }

        m_DisableFontTextureRebuiltCallback = false;
    }

    protected void UpdateImagesVertexIndex(string text,int charCount)
    {
        myImageVertexIndex.Clear();
        if(string.IsNullOrEmpty(text))
        {
            return;
        }

        foreach (Match match in smyRegex.Matches(text))
        {
            int startIndex = match.Index;
            if(startIndex >= charCount)
            {
                return;
            }
            int endIndex = startIndex + match.Length;
            if(endIndex > charCount)
            {
                endIndex = charCount;
            }
            myImageVertexIndex.Add(startIndex);
            myImageVertexIndex.Add(endIndex);
        }
    }

    public string Format(string text)
    {
        if(atlasData == null)
        {
            return text;
        }
        myStringBuilder.Length = 0;
        myStringBuilder.Append(text);

        int count = myStringBuilder.Length;
        bool start = false;
        int startIndex = 0;
        for (int i = 0; i < count; i++)
        {
            if (start)
            {
                if (myStringBuilder[i] == ']')
                {
                    string imageName = myImageNameBuilder.ToString();
                    ImageData imageData = atlasData.GetImageData(imageName);
                    if (imageData != null)
                    {
                        myImageNameBuilder.Length = 0;
                        string quad = myImageNameBuilder.AppendFormat(myQuadFormat, fontSize, imageData.uv.x, imageData.uv.y, imageData.uv.width * MyAtlasAspect, imageData.uv.height).ToString();
                        int matchLength = i - startIndex + 1;
                        int quadLength = quad.Length;
                        myStringBuilder.Remove(startIndex, matchLength);
                        myStringBuilder.Insert(startIndex, quad);
                        count = myStringBuilder.Length;
                        i = startIndex + quadLength - 1;
                    }
                    else
                    {
                        string json = "{" + imageName + "}";
                        imageInfo.Reset();
                        try
                        {
                            Newtonsoft.Json.JsonConvert.PopulateObject(json, imageInfo);
                            //JsonUtility.FromJsonOverwrite(json, imageInfo);
                        }
                        catch (Exception e)
                        {
                            //Debug.LogException(e);
                            start = false;
                            continue;
                        }
                        imageData = atlasData.GetImageData(imageInfo.name);
                        int size = imageInfo.size;
                        if(size == 0)
                        {
                            size = fontSize;
                        }
                        else if (size == -1)
                        {
                            size = (int)imageData.size.y;
                        }
                        if (imageData != null)
                        {
                            myImageNameBuilder.Length = 0;
                            string quad = myImageNameBuilder.AppendFormat(myQuadFormat, size, imageData.uv.x, imageData.uv.y, imageData.uv.width * MyAtlasAspect, imageData.uv.height).ToString();
                            int matchLength = i - startIndex + 1;
                            int quadLength = quad.Length;
                            myStringBuilder.Remove(startIndex, matchLength);
                            myStringBuilder.Insert(startIndex, quad);
                            count = myStringBuilder.Length;
                            i = startIndex + quadLength - 1;
                        }
                    }
                    start = false;
                    continue;
                }

                myImageNameBuilder.Append(myStringBuilder[i]);
            }

            if(myStringBuilder[i] == '[')
            {
                startIndex = i;
                myImageNameBuilder.Length = 0;
                start = true;
            }
        }
        return myStringBuilder.ToString();
    }

    public override float preferredWidth
    {
        get
        {
            var settings = GetGenerationSettings(Vector2.zero);
            return cachedTextGeneratorForLayout.GetPreferredWidth(realText, settings) / pixelsPerUnit;
        }
    }

    public override float preferredHeight
    {
        get
        {
            var settings = GetGenerationSettings(new Vector2(GetPixelAdjustedRect().size.x, 0.0f));
            return cachedTextGeneratorForLayout.GetPreferredHeight(realText, settings) / pixelsPerUnit;
        }
    }

#if UNITY_EDITOR
    protected override void OnValidate()
    {
        base.OnValidate();
        isTextDirty = true;
        if(myImage && atlasData)
        {
            myImage.MyTexture = atlasData.myAtlas;
        }
        UpdateAspect();
    }

    [ContextMenu("Copy Real Text")]
    void CopyRealText()
    {
        EditorGUIUtility.systemCopyBuffer = realText;
    }

    [ContextMenu("Fill All Image")]
    void Fill()
    {
        if (myAtlasData != null)
        {
            string s = "";
            foreach (var imageData in myAtlasData.myImageDatas)
            {
                s += "[name:\"" + imageData.name + "\",size:0]";
            }
            text += s;
        }
    }

#endif
}
