using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;
using UnityEngine;

public class Building {

    public List<GmlPolygon> polys = new List<GmlPolygon>();

    public Color color = Color.blue;
    public List<GameObject> sides {
        get {
            List<GameObject> sides = new List<GameObject>();
            foreach (GmlPolygon item in polys) {
                sides.Add(item.Side());
            }

            return sides;
        }
    }

    public GameObject build() {
        GameObject build = new GameObject("building");
        build.isStatic = true;
        foreach (GmlPolygon item in polys) {
            item.Side().transform.parent = build.transform;
        }
        return build;
    }

    public MeshFilter[] buildOptiMI(Transform parent) {
        //GameObject build = new GameObject("building");
        //build.isStatic = true;


        MeshFilter[] filters = new MeshFilter[polys.Count];

        for (int i = 0; i < polys.Count; i++) {
            GameObject temp = polys[i].MiConvexHull();
            temp.transform.parent = parent;

            filters[i] = temp.GetComponent<MeshFilter>();
        }



        return filters;
    }


    public MeshFilter[] buildOptiPTM(Transform parent) {
        //GameObject build = new GameObject("building");
        //build.isStatic = true;


        MeshFilter[] filters = new MeshFilter[polys.Count];

        for (int i = 0; i < polys.Count; i++) {
            GameObject temp = polys[i].polyToMesh();
            temp.transform.parent = parent;

            filters[i] = temp.GetComponent<MeshFilter>();
        }



        return filters;
    }

    public MeshFilter[] buildOptiTest(Transform parent,Vector3 reference) {
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

    public MeshFilter[] buildOpti(Transform parent) {
        //GameObject build = new GameObject("building");
        //build.isStatic = true;


        MeshFilter[] filters = new MeshFilter[polys.Count];

        for (int i = 0; i < polys.Count; i++) {
            GameObject temp = polys[i].SideOpti();
            temp.transform.parent = parent;
            
            filters[i] = temp.GetComponent<MeshFilter>();
        }


        
        return filters;
    }

    public void newPolygon(XElement poly) {
        polys.Add(new GmlPolygon(poly));
    }
    public override string ToString() {
        string output = "";

        output += "Polygon [  ";
        foreach (var item in polys) {
            output += item + "\n ";
        }
        output += " ]";

        return output;
    }
}
