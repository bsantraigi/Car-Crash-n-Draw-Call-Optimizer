using UnityEngine;
using System.Collections.Generic;

public class DrawCallOptimizer : MonoBehaviour {
    public GameObject EmptyCombinedPrefab;
    const int MAX_VERTEX_COUNT = 65000;
    delegate T Action<T>(T item);
    struct TextureInfo
    {
        public float scaleX, scaleY;
        public static int AtlasGridSize;

        public static TextureInfo create(Texture t, int pixelsPerRow)
        {
            TextureInfo ti = new TextureInfo();
            ti.scaleX = (float)t.width / pixelsPerRow;
            ti.scaleY = (float)t.height / pixelsPerRow;
            return ti;
        }
    }

	// Use this for initialization
	void Start () {
        Transform[] children = GetComponentsInChildren<Transform>();
        List<Transform> transformsList = new List<Transform>();
        List<Mesh> meshes = new List<Mesh>();
        Dictionary<string, int> textureNameMap = new Dictionary<string, int>();
        Dictionary<int, Texture> textureDictionary = new Dictionary<int, Texture>();
        List<int> textureIds = new List<int>();
        for (int i = 0; i<children.Length; i++)
        {
            MeshFilter m = children[i].gameObject.GetComponent<MeshFilter>();
            if(m != null)
            {
                meshes.Add(m.mesh);
                transformsList.Add(children[i]);
                Texture texture = children[i].GetComponent<MeshRenderer>().sharedMaterial.mainTexture;
                if (texture != null)
                {
                    string name = texture.name;
                    
                    if (!textureNameMap.ContainsKey(name))
                    {
                        int id = textureNameMap.Keys.Count;
                        textureNameMap.Add(name, id);
                        textureDictionary.Add(id, texture);
                        textureIds.Add(id);
                    }
                    else
                    {
                        textureIds.Add(textureNameMap[name]);
                    }
                }
                else
                {
                    textureIds.Add(-1);
                }
            }
        }

        //foreach (string s in textureNameMap.Keys)
        //{
        //    Debug.Log("Texture Found: " + s);
        //}

        int size = Mathf.CeilToInt(Mathf.Sqrt(textureDictionary.Count));
        TextureInfo.AtlasGridSize = size;

        GameObject combined;
        if (EmptyCombinedPrefab != null)
        {
            combined = Instantiate(EmptyCombinedPrefab, Vector3.zero, Quaternion.identity) as GameObject;
        }
        else
        {
            combined = new GameObject();
            combined.AddComponent<MeshFilter>();
            combined.AddComponent<MeshRenderer>();
        }

        combined.name = "Combined GameObject";
        combined.transform.parent = transform;
        TextureInfo[] infos = new TextureInfo[textureDictionary.Count];
        //AtlasLink atlasLink = new AtlasLink(new Texture2D(12,12), 2);
        //combined.GetComponent<MeshRenderer>().material.mainTexture = atlasLink.AtlasRef;

        MeshFilter mf = combined.GetComponent<MeshFilter>();

        mergeMeshes(ref mf, meshes, transformsList, infos, textureIds.ToArray()).name = "CombinedMesh";
        for(int i = 0; i <transformsList.Count; i++)
        {
            Destroy(transformsList[i].gameObject);
        }
        // gameObject.SetActive(false);
    }

    Mesh mergeMeshes(ref MeshFilter mf, List<Mesh> meshes, List<Transform> transforms, TextureInfo[] infos, int[] textureIds)
    {
        Mesh combinedMesh = new Mesh();
        
        int i = 0;
        int vertCount = 0;
        int triCount = 0;
        for (i = 0; i < meshes.Count; i++)
        {
            vertCount += meshes[i].vertices.Length;
            triCount += meshes[i].triangles.Length;
        }
        Vector3[] vertices = new Vector3[vertCount];
        Vector2[] uvs = new Vector2[vertCount];
        int[] triangles = new int[triCount];

        int vertCurrentSize = 0;
        int triCurrentSize = 0;
        for (i = 0; i < meshes.Count; i++)
        {
            if(vertCurrentSize > MAX_VERTEX_COUNT)
            {
                Debug.LogError("Vertex Limit Exceeded.");
            }
            Mesh m = meshes[i];
            Transform root = transforms[i];

            // Texture infos to be set
            TextureInfo info = new TextureInfo();
            int textureIndex = 0;
            Vector2 offset = Vector3.zero;
            //=============================
            if (textureIds[i] != -1)
            {
                info = infos[textureIds[i]];
                textureIndex = textureIds[i];
                offset = new Vector2(textureIndex % TextureInfo.AtlasGridSize, Mathf.Floor(textureIndex / TextureInfo.AtlasGridSize));
                offset /= (float)TextureInfo.AtlasGridSize;
            }
            
            Copy<Vector3>(m.vertices, ref vertices, vertCurrentSize, m.vertices.Length, delegate(Vector3 v) { return root.TransformPoint(v); });
            Copy<Vector2>(m.uv, ref uvs, vertCurrentSize, m.vertices.Length,
                delegate(Vector2 v) 
                {
                    return new Vector2(v.x * info.scaleX, v.y * info.scaleY) + offset;
                });
            Copy<int>(m.triangles, ref triangles, triCurrentSize, m.triangles.Length, delegate(int k) { return k + vertCurrentSize; });

            vertCurrentSize += m.vertices.Length;
            triCurrentSize += m.triangles.Length;
        }

        combinedMesh.vertices = vertices;
        combinedMesh.uv = uvs;
        combinedMesh.triangles = triangles;
        combinedMesh.RecalculateNormals();
        mf.sharedMesh = combinedMesh;
        return combinedMesh;
    }


    /*Texture2D mergeTextures(Dictionary<int, Texture> textureDictionary, ref TextureInfo[] infos)
    {
        int size = TextureInfo.AtlasGridSize;
        int blockWidth = 0;
        for(int id = 0; id < textureDictionary.Keys.Count; id++)
        {
            Texture t = textureDictionary[id];
            blockWidth = Mathf.Max(blockWidth, t.height, t.width);
        }
        Texture2D atlas = new Texture2D(blockWidth * size, blockWidth * size);
        Color[] pixels = new Color[blockWidth * size * blockWidth * size];
        int i = 0;
        int blockSize = blockWidth * blockWidth;
        int atlasSize = blockWidth * size;
        Debug.Log(blockWidth + " , " + blockWidth);


        for (int id = 0; id < textureDictionary.Keys.Count; id++)
        {
            Texture2D t = textureDictionary[id] as Texture2D;
            infos[id] = TextureInfo.create(t, atlasSize);
            Color[] source = t.GetPixels();
            int horizontalShift = (i % size) * blockWidth;
            for (int h = 0; h < t.height; h++)
            {
                int offset = atlasSize * h;
                int sourceOffset = h * t.width;
                for (int w = 0; w < t.width; w++)
                {
                    pixels[offset + w + horizontalShift] = source[sourceOffset + w];
                }
            }
            i++;
        }
        atlas.SetPixels(pixels);
        atlas.filterMode = FilterMode.Point;
        atlas.Apply();

        return atlas;
    }*/
    
    static void Copy<T>(T[] source, ref T[] dest, int destStart, int size, Action<T> action = null)
    {
        if(dest == null || source == null)
        {
            Debug.LogError("Null reference sent ot Copy<T> function.");
            return;
        }
        int otherIndex = 0;
        if (action == null)
        {
            for (int i = destStart; i < destStart + size; i++)
            {
                dest[i] = source[otherIndex ++];
            }
        }
        else
        {
            for (int i = destStart; i < destStart + size; i++)
            {
                dest[i] = action(source[otherIndex++]);
            }
        }
    }
}
