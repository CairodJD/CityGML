using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;
using UnityEngine;
using TriangleNet;
using TriangleNet.Geometry;
using Mesh = UnityEngine.Mesh;
using TriangleNet.Meshing;
using TriangleNet.Meshing.Algorithm;
using TriangleNet.Topology;
using MIConvexHull;

public class GmlPolygon {
    TriangleNet.Meshing.ConstraintOptions options =
    new TriangleNet.Meshing.ConstraintOptions() { ConformingDelaunay = true };



    public Polygon polygon = new Polygon();

    public List<float> elevation = new List<float>();


    public List<Vector3> points = new List<Vector3>();

    public Color polyColor = Color.gray;

    public Poly2Mesh.Polygon ptmpolygon = new Poly2Mesh.Polygon();
    public string name;

    public GmlPolygon(XElement poly) {
        // separer les valeurs dans un tab
        points.Clear();
        string[] coords = poly.Value.Split(new char[0], StringSplitOptions.RemoveEmptyEntries);
        // recup les corrds par stack de 3
        for (int i = 0; i < coords.Length; i += 3) {
            points.Add(new Vector3(
                Get<float>(coords[i].Replace('.', ',')),
               Get<float>(coords[i + 1].Replace('.', ',')),
                Get<float>(coords[i + 2].Replace('.', ',')))
                );
            elevation.Add(Get<float>(coords[i + 2].Replace('.', ',')));
            polygon.Points.Add(new TriangleNet.Geometry.Vertex(
                Get<double>(coords[i].Replace('.', ',')),
                Get<double>(coords[i + 1].Replace('.', ','))
                )
                );

        }

    }
    public GmlPolygon(XElement poly , Color newC) {
        polyColor = newC;
        // separer les valeurs dans un tab
        points.Clear();
        string[] coords = poly.Value.Split(new char[0], StringSplitOptions.RemoveEmptyEntries);
        // recup les corrds par stack de 3
        for (int i = 0; i < coords.Length; i+=3) {
            points.Add(new Vector3(
                Get<float>(coords[i].Replace('.', ',')),
               Get<float>(coords[i+1].Replace('.', ',')),
                Get<float>(coords[i+2].Replace('.', ','))) 
                );

            //TRIANGLE NET
            elevation.Add(Get<float>(coords[i + 2].Replace('.', ',')));
            polygon.Points.Add(new TriangleNet.Geometry.Vertex(
                Get<double>(coords[i].Replace('.', ',')),
                Get<double>(coords[i + 1].Replace('.', ','))
                )
                );



        }
        
    }

    //non opitmize version returning a gameobject for each side
    public GameObject Side() {
        GameObject side = new GameObject();
        side.isStatic = true;
        Mesh mesh = new Mesh();
        Vector3[] vertices =points.ToArray();
        // nombre de points divisé par 2 * 2 * 3
        int[] triangles = new int[Mathf.CeilToInt(points.Count/2) * 6 ];

        int tri = 0;
        for (int i = 0; i < Mathf.CeilToInt(points.Count / 2); i += 3) {
            triangles[tri] = i;
            triangles[tri++] = i + 1;
            triangles[tri++] = i + 2;
            // oposite
            triangles[tri++] = i;
            triangles[tri++] = i + 2;
            triangles[tri++] = i + 3;
        }



        mesh.vertices = vertices;
        mesh.triangles = triangles;
        //mesh.RecalculateBounds();
        //mesh.RecalculateNormals();

        MeshFilter mshFilter = side.AddComponent<MeshFilter>();
        side.AddComponent<MeshRenderer>().material.color = polyColor;
        mshFilter.mesh = mesh;
        
        return side;
    }


    public GameObject ConvexHullTest() {
        GameObject side = new GameObject();
        side.isStatic = true;
        Mesh mesh = new Mesh();
        Vector3[] vertices = points.ToArray();
        // nombre de points divisé par 2 * 2 * 3
        int[] triangles = new int[Mathf.CeilToInt(points.Count / 2) * 6];
        //Linear ring

        triangles = ConvexHull.Generate(vertices);


        mesh.vertices = vertices;
        //mesh.triangles = triangles;
        mesh.triangles = triangles;
        //mesh.RecalculateBounds();
        //mesh.RecalculateNormals();

        MeshFilter mshFilter = side.AddComponent<MeshFilter>();
        //side.AddComponent<MeshRenderer>().material.color = polyColor;
        mshFilter.mesh = mesh;

        return side;
    }

    //Naive first implementation 
    //we get a prety accurate representation but there a too many unwanted holes 
    public GameObject SideOpti() {
        GameObject side = new GameObject();
        side.isStatic = true;
        Mesh mesh = new Mesh();
        Vector3[] vertices = points.ToArray();
        // nombre de points divisé par 2 * 2 * 3
        int[] triangles = new int[Mathf.CeilToInt(points.Count / 2) * 6];
        //Linear ring

        Debug.Log(points.Count);
        //fau trouver un truc qui marche avec les BUILDINGs
        // CHERCHE COMMENT ON DRAW UN LINEAR RING
        int tri = 0;
        for (int i = 0; i < Mathf.CeilToInt(points.Count / 2); i += 3) {
            triangles[tri] = i;
            triangles[tri++] = i + 1;
            triangles[tri++] = i + 2;
            // oposite
            triangles[tri++] = i;
            triangles[tri++] = i + 2;
            triangles[tri++] = i + 3;
        }


   

        mesh.vertices = vertices;
        //mesh.triangles = triangles;
        mesh.triangles = triangles;
        //mesh.RecalculateBounds();
        //mesh.RecalculateNormals();

        MeshFilter mshFilter = side.AddComponent<MeshFilter>();
        side.AddComponent<MeshRenderer>().material.color = polyColor;
        mshFilter.mesh = mesh;

        return side;
    }

    //Regulare 2d delauney triangulation but we rotate 2d points to get the z axis
    //Kinda works but with test data large scale data no ??
    public GameObject polyToMesh() {
        ptmpolygon.outside = points;
        GameObject side = new GameObject();
        side.isStatic = true;
        Mesh mesh = Poly2Mesh.CreateMesh(ptmpolygon);

        MeshFilter mshFilter = side.AddComponent<MeshFilter>();
        //side.AddComponent<MeshRenderer>().material.color = polyColor;
        mshFilter.mesh = mesh;

        return side;
    }

    //Convexhull issnt the approache to use
    public GameObject MiConvexHull() {
        GameObject side = new GameObject();
        side.isStatic = true;
        Mesh mesh = new Mesh();
        List<Vector3> vertices = new List<Vector3>();
        // nombre de points divisé par 2 * 2 * 3
        List<int> triangles = new List<int>();

        List<DefaultVertex> pos = new List<DefaultVertex>();
        for (int i = 0; i < points.Count; i++) {
            DefaultVertex v = new DefaultVertex();

            v.Position = new double[] { (double)points[i].x, (double)points[i].y, (double)points[i].z };
            pos.Add(v);
        }

        //Debug.Log(pos.Count);
        var faces = Triangulation.CreateDelaunay(pos);

        foreach (var c in faces.Cells) {
            Debug.Log(c.Vertices.Length);
        }
        
        


        mesh.vertices = vertices.ToArray();
        //mesh.triangles = triangles;
        mesh.triangles = triangles.ToArray();

        MeshFilter mshFilter = side.AddComponent<MeshFilter>();
        //side.AddComponent<MeshRenderer>().material.color = polyColor;
        mshFilter.mesh = mesh;

        return side;
    }


    //Voronoir meshses with "elevation" to get z axis
    // constuire un mesh avec les points trianguler de l'algo + " l'evalation " en z
    // le point z doit etre to local
    // on garde ca
    //http://www.cs.cmu.edu/~quake/triangle.html
    public GameObject MeshFromTusais(Vector3 reference) {
        GameObject side = new GameObject();
        side.isStatic = true;
        Mesh mesh = new Mesh();
        List<Vector3> vertices = new List<Vector3>();
        // nombre de points divisé par 2 * 2 * 3
        List<int> triangles = new List<int>();
        //Linear ring
        //Debug.Log("tu sais " + polygon.Points.Count);
        TriangleNet.Mesh tmesh = Mesh(reference);
        IEnumerator<Triangle> triangleEnumerator = tmesh.Triangles.GetEnumerator();


        for (int i = 0; i < tmesh.Triangles.Count; i++) {
            if (!triangleEnumerator.MoveNext()) {
                // If we hit the last triangle before we hit the end of the chunk, stop
                break;
            }
            // Get the current triangle
            Triangle triangle = triangleEnumerator.Current;

            // For the triangles to be right-side up, they need
            // to be wound in the opposite direction
            Vector3 v0 = GetPoint3D(tmesh,triangle.vertices[0].id);
            Vector3 v1 = GetPoint3D(tmesh,triangle.vertices[1].id);
            Vector3 v2 = GetPoint3D(tmesh,triangle.vertices[2].id);

            // This triangle is made of the next three vertices to be added
            triangles.Add(vertices.Count);
            triangles.Add(vertices.Count + 1);
            triangles.Add(vertices.Count + 2);

            // Add the vertices
            vertices.Add(v0);
            vertices.Add(v1);
            vertices.Add(v2);
        }


        mesh.vertices = vertices.ToArray();
        //mesh.triangles = triangles;
        mesh.triangles = triangles.ToArray();
        //mesh.RecalculateBounds();
        //mesh.RecalculateNormals();

        MeshFilter mshFilter = side.AddComponent<MeshFilter>();
        side.AddComponent<MeshRenderer>().material.color = polyColor;
        mshFilter.mesh = mesh;

        return side;
    }

   
    public Vector3 GetPoint3D(TriangleNet.Mesh mesh, int index) {
        Vertex vertex = mesh.vertices[index];
        return new Vector3((float)vertex.x,(float)vertex.y, elevation[index]);
    }

    public TriangleNet.Mesh Mesh(Vector3 refpoiint) {

        //polygon.Points = City.ToLocal(polygon.Points, refpoiint);
        Tuple<List<Vertex>, List<float>> info = City.ToLocal(polygon.Points, elevation, refpoiint);
        this.elevation = info.Item2;
        //Debug.Log(" gmlPoly mesh " + polygon.Points.Count);

        return (TriangleNet.Mesh) new Dwyer().Triangulate(info.Item1, new Configuration());
    }



    public GmlPolygon() {
        
    }
    public GmlPolygon(List<Vector3> points) {
        this.points = points;
    }

    private T Get<T>(string input) {
        return (T)Convert.ChangeType(input, typeof(T));
    }

    public override string ToString() {
        string output = "";

        output += "PTS [ ";
        foreach (var item in points) {
            output += item+ " \n";
        }
        output += " ]";

        return output;
    }
}
