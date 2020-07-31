using System;
using System.IO;
using System.Xml.Linq;
using System.Xml;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TriangleNet.Geometry;
using UnityEditor;


public class City : MonoBehaviour {
    XNamespace gml = "http://www.opengis.net/gml";
    XNamespace bldg = "http://www.opengis.net/citygml/building/2.0";
    string extension = ".gml";
    public string file = "LYON_6EME_PONT_2015";
    XDocument cityData;


    Vector3 vScale;

    List<Building> buildings = new List<Building>();
    List<GmlPolygon> all = new List<GmlPolygon>();
    List<GmlPolygon> findRef = new List<GmlPolygon>();


    public bool Opti = false;


    Vector3 tktrefpoint;

    private void Start() {
        ReaD();
        
    }

    
    //Create xDocument from file
    private void ReaD() {
        
        cityData = XDocument.Load(Application.dataPath + "/Data/building/" + file + extension);
        XNamespace def = cityData.Root.GetDefaultNamespace();


        //Chercher tous les city object member 
        // differencier building et ground
        Debug.Log(cityData.Descendants(def + "cityObjectMember").Count());

        foreach (XElement cityObject in cityData.Descendants(def + "cityObjectMember")) {
            if (cityObject.Descendants(bldg + "Building").Any()) {
                Building temp = new Building();
                foreach (XElement poly in cityObject.Descendants(gml + "posList")) {
                    //Debug.Log(poly.)
                    findRef.Add(new GmlPolygon(poly));
                    temp.polys.Add(new GmlPolygon(poly, Color.blue));
                }
                buildings.Add(temp);
            } else {
                foreach (XElement poly in cityObject.Descendants(gml + "posList")) {
                    GmlPolygon tkt = new GmlPolygon(poly);
                    findRef.Add(tkt);
                    all.Add(tkt);
                }
            }
        }
        //Debug.Log(cityData.Descendants(def+"cityObjectMember").Count());

        Vector3 refpoint = ReferencePoint(findRef);
        tktrefpoint = refpoint;
        buildings = MoveToLocal(buildings, refpoint);
        //all = ToLocal(all, refpoint);

        //Mettre ca dans une un seul GameObject qui combine les instances


        if (Opti) {
            // un batch par building
            //foreach (Building bl in buildings) {
            //    MeshFilter[] meshes = bl.buildOpti(transform);
            //    CombineMeshes(meshes, Color.blue).transform.parent = gameObject.transform;
            //}
            foreach (Building bl in buildings) {
                //MeshFilter[] meshes = bl.buildOptiTest(transform, tktrefpoint);
                MeshFilter[] meshes = bl.buildOpti(transform);
                //MeshFilter[] meshes = bl.buildOptiConvexHull(transform);
                //MeshFilter[] meshes = bl.buildOptiPTM(transform);
                //MeshFilter[] meshes = bl.buildOptiMI(transform);
                CombineMeshes(meshes, Color.blue).transform.parent = gameObject.transform;
            }
        } else {
            foreach (Building bl in buildings) {
                bl.build().transform.parent = gameObject.transform;
            }
        }




        //Combiner les meshes du terrain aussi

        MeshFilter[] Nobuilding = GetMeshes(all, transform, tktrefpoint);
        CombineMeshes(Nobuilding, Color.black).transform.parent = gameObject.transform;

        //foreach (GmlPolygon poly in all) {

        //    GameObject side = poly.SideOpti();
        //    side.transform.parent = this.gameObject.transform;
        //}
        gameObject.transform.Rotate(-90, 0, 0);
    }

    public MeshFilter[] GetMeshes( List<GmlPolygon> polys, Transform parent, Vector3 reference) {
        //GameObject build = new GameObject("building");
        //build.isStatic = true;


        MeshFilter[] filters = new MeshFilter[polys.Count];

        for (int i = 0; i < polys.Count; i++) {
            GameObject temp = polys[i].MeshFromTusais(reference);
            temp.transform.parent = parent;

            filters[i] = temp.GetComponent<MeshFilter>();
        }



        return filters;
    }

    private GameObject CombineMeshes(MeshFilter[] meshes,Color color) {
        GameObject combined = new GameObject("Combined Meshes");
        MeshRenderer renderer = combined.AddComponent<MeshRenderer>();

        MeshFilter meshFilter = combined.AddComponent<MeshFilter>();
        meshFilter.mesh = new Mesh();

        CombineInstance[] combine = new CombineInstance[meshes.Length];

        for (int i = 0; i < meshes.Length; i++) {
            combine[i].mesh = meshes[i].sharedMesh;
            combine[i].transform = meshes[i].transform.localToWorldMatrix;
            meshes[i].gameObject.SetActive(false);
            meshes[i].gameObject.transform.parent = combined.transform;
        }
        renderer.material.color = color;
        meshFilter.mesh.CombineMeshes(combine);
        return combined;
            
    }

    private T Get<T>(string input) {
        return (T)Convert.ChangeType(input, typeof(T));
    }

    //To implement the above conversion, all points in the project are found using:
    //    tree.findall('.//{%s}posList' % ns_gml)
    //    and then to perform the coordinates conversion the following functions are defined and used:
    //    find_reference_point(l), which takes a takes a list of lists as input and will return the corner point
    //    as a list(point with least x and least y and least z). similarly, the function find_max_point(l) is
    //    defined which will take the maximum value instead.
    //    The function move_to_local(local_pont, l) converts a list of points (l) into local coordinates by
    //    subtracting the local point value (local_pont) from them.
    Vector3 ReferencePoint(List<GmlPolygon> all) {
        Vector3 refpoint = Vector3.positiveInfinity;
        foreach (GmlPolygon poly in all) {
            foreach (Vector3 item in poly.points) {
                refpoint = (item.magnitude < refpoint.magnitude) ? item : refpoint;
            }
        }

        return refpoint;
    }

    Vector3 ReferencePoint(List<Building> buildings) {
        Vector3 refpoint = Vector3.positiveInfinity;
        foreach (Building building in buildings) {
            foreach (GmlPolygon polygon in building.polys) {
                foreach (Vector3 item in polygon.points) {
                    refpoint = (item.magnitude < refpoint.magnitude) ? item : refpoint;
                }
            }
        }

        return refpoint;
    }

    List<GmlPolygon> ToLocal(List<GmlPolygon> points, Vector3 refpoint) {
        List<GmlPolygon> local = new List<GmlPolygon>();

        foreach (GmlPolygon item in points) {
            GmlPolygon newPoly = new GmlPolygon();
            foreach (Vector3 p in item.points) {
                newPoly.points.Add(p-refpoint); 
            }
            local.Add(newPoly);
        }    
        return local;
    }


    public static Tuple<List<Vertex>,List<float>> ToLocal(List<Vertex> points,List<float> elevation, Vector3 refpoint) {
        List<Vertex> local = new List<Vertex>();
        List<float> localelevation = new List<float>();

        for (int i = 0; i < points.Count; i++) {
            Vertex a = new Vertex(points[i].x - refpoint.x, points[i].y - refpoint.y);
            float b = elevation[i] - refpoint.z;

            localelevation.Add(b);
            local.Add(a);
        }


        return new Tuple<List<Vertex>, List<float>>(local,localelevation);
    }

    List<Vector3> ToLocal(List<Vector3> points , Vector3 refpoint) {
        List<Vector3> local = new List<Vector3>();
        foreach (Vector3 item in points) {
            local.Add(item - refpoint);
        }

        return local;
    }

    List<Building> MoveToLocal(List<Building> buildings , Vector3 refpoint) {
        List<Building> bld = buildings;

        foreach (Building building in buildings) {
            foreach (GmlPolygon polygon in building.polys) {
                polygon.points = ToLocal(polygon.points, refpoint);
                //polygon.polygon.Points = ToLocal(polygon.polygon.Points, refpoint);
            }
        }

        return bld;
    }




    private void OnDrawGizmos() {
        Handles.color = Color.black;
        Gizmos.color = Color.blue;
        if (buildings != null && buildings.Count > 0 && tktrefpoint != null) {

            foreach (Building bld in buildings) {
                foreach (GmlPolygon polygon in bld.polys) {
                    //Gizmos.color = UnityEngine.Random.ColorHSV();
                    foreach (Vector3 item in polygon.points) {
                        Gizmos.DrawWireSphere(item, 0.5f);
                    }
                }

            }
        }
    }
}
