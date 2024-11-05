using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using Random = UnityEngine.Random;

public class ManipulatorAgent : Agent
{
    // Конец щупальца
    public Transform head;
    // Цель, которой необходимо коснуться
    public Transform target;
    // Настройки области спауна цели для обучения
    public Vector3 targetSpawnCenter = new Vector3(0, 1.7f, 0);
    public Vector3 targetSpawnScale = new Vector3(2f, 1.5f, 2f);
    public float targetCenterOffset = 0;
    public bool drawTargetGizmos = false;
    private JointController[] joints;
    public override void Initialize()
    {
        joints = GetComponentsInChildren<JointController>();
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var continuousActionsOut = actionsOut.ContinuousActions;
        continuousActionsOut[0] = -Input.GetAxis("Horizontal");
        continuousActionsOut[1] = Input.GetAxis("Vertical");
        continuousActionsOut[2] = -Input.GetAxis("Mouse X");
        continuousActionsOut[3] = Input.GetAxis("Mouse Y");
    }
    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        for (var i = 0; i < joints.Length; i++)
            joints[i].Rotate(actionBuffers.ContinuousActions[i] * 5f);
        var newDist = (head.transform.position - target.transform.position).magnitude;
        if (newDist < 0.4f)
            EndEpisode();
    }
    public override void CollectObservations(VectorSensor sensor)
    {
    }
    public override void OnEpisodeBegin()
    {
        target.transform.position = randomTargetPosititon();
    }

    private Vector3 randomTargetPosititon()
    {
        var point = Random.insideUnitSphere;
        point.Scale(targetSpawnScale);
        point += targetSpawnCenter;
        var vec2 = new Vector2(point.x, point.z);
        if  (vec2.magnitude < targetCenterOffset) {
            return randomTargetPosititon();
        }
        return transform.position + point;
    }
    public void OnDrawGizmosSelected()
    {
        if (!drawTargetGizmos) return;
        Gizmos.color = new Color(1, 0, 0, 0.75F);
        for (var i = 0; i < 100; i++)
        {
            Gizmos.DrawWireSphere(randomTargetPosititon(), 0.1f);
        }
    }
}
