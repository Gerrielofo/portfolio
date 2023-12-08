using UnityEngine;

public class SprayPaint : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] Transform _shootPos;
    public Color[] colors;

    public Mesh curMesh;

    RaycastHit _hit;

    private void Update()
    {
        Ray ray = new Ray(_shootPos.position, transform.forward);
        if (Physics.Raycast(ray, out _hit))
        {
            Mesh mesh = _hit.transform.GetComponent<MeshFilter>().mesh;
            if (mesh)
            {
                curMesh = mesh;
                if (PaintPlayer.spray)
                {
                    PaintCar(curMesh, _hit.triangleIndex, colors[QuestSystem.colorIndex]);
                }
            }
            else
            {
                Debug.Log("No mesh found, check if the Read/Write is enabled!");
            }
        }
    }

    public void PaintCar(Mesh mesh, int i, Color color)
    {
        Vector3[] _vertices = mesh.vertices;
        Color[] _colors = mesh.colors;

        Debug.Log(_colors.Length);

        if (_colors.Length < 1)
        {
            _colors = new Color[_vertices.Length];
            for (int j = 0; j < _colors.Length; j++)
            {
                _colors[j] = Color.blue;
            }
        }

        int[] _triangles = mesh.triangles;
        for (int t = 0; t < _triangles.Length; t++)
        {
            if (_triangles[t] == i) // closest vertex i
            {

                Debug.Log("Colors: " + _colors.Length + ", t: " + t);
                int subIndex = t % 3;
                if (subIndex == 0)
                {
                    // apply color to t+1 & t+2
                    _colors[t + 1] = color;
                    _colors[t + 2] = color;
                }
                else if (subIndex == 1)
                {
                    // apply color to t-1 and t+1
                    _colors[t - 1] = color;
                    _colors[t + 1] = color;
                }
                else
                {
                    // apply color to t-2 & t-1
                    _colors[t - 2] = color;
                    _colors[t - 1] = color;
                }
            }
        }
        mesh.colors = _colors;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawRay(_shootPos.position, _hit.point);
    }
}