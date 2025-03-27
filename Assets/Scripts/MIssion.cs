using UnityEngine;

[CreateAssetMenu(fileName = "NewMission", menuName = "Missions/Mission")]
public class Mission : ScriptableObject
{
    public string missionID;
    public string description;
    public int coinReward;
    public int xpReward;
}