using UnityEngine;

public class BuildManager : MonoBehaviour
{
	public static BuildManager Instance;

	void Awake()
	{
		if (Instance != null)
		{
			Debug.LogError("More than one BuildManager in scene!");
			return;
		}
		Instance = this;
	}

	public GameObject buildEffect;
	public GameObject sellEffect;

	private TurretBlueprint turretToBuild;
	private Nodes selectedNode;

	public NodeUI nodeUI;

	public bool CanBuild { get { return turretToBuild != null; } }
	public bool HasMoney { get { return PlayerStats.Money >= turretToBuild.cost; } }

	public void SelectNode(Nodes node)
	{
		if (selectedNode == node)
		{
			DeselectNode();
			return;
		}

		selectedNode = node;
		turretToBuild = null;

		nodeUI.SetTarget(node);
	}

	public void DeselectNode()
	{
		selectedNode = null;
		NodeUI.Hide();
	}

	public void SelectTurretToBuild(TurretBlueprint turret)
	{
		turretToBuild = turret;
		DeselectNode();
	}

	public TurretBlueprint GetTurretToBuild()
	{
		return turretToBuild;
	}

}
