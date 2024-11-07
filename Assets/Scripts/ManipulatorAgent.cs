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

    // Расстояние для достижения цели
    public float targetReachThreshold = 0.4f;

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
        // Применяем действия к каждому суставу щупальца
        for (var i = 0; i < joints.Length; i++)
        {
            joints[i].Rotate(actionBuffers.ContinuousActions[i] * 5f);
        }

        // Вычисляем расстояние до цели
        float distanceToTarget = Vector3.Distance(head.position, target.position);

        // Если щупальце достигает цели, завершаем эпизод и даем вознаграждение
        if (distanceToTarget < targetReachThreshold)
        {
            SetReward(MaxStep - StepCount);
            EndEpisode();
        }
        else
        {
            // Меньшее вознаграждение, если цель не достигнута, но агент приближается
            SetReward(-StepCount / (float)MaxStep);
        }
    }


    public override void CollectObservations(VectorSensor sensor)
    {
        // Наблюдения для агентского обучения
        // 1. Положение цели (относительно щупальца)
        sensor.AddObservation((target.position - head.position) + new Vector3(
            Random.Range(-0.1f, 0.1f),
            Random.Range(-0.1f, 0.1f),
            Random.Range(-0.1f, 0.1f)
        ));

        // 2. Положение и ориентация самого щупальца
        sensor.AddObservation(head.position);
        sensor.AddObservation(head.rotation);

        // 3. Состояние каждого из суставов щупальца
        foreach (var joint in joints)
        {
            sensor.AddObservation(joint.transform.localRotation);
        }
    }

    /*
    public override void CollectObservations(VectorSensor sensor)
    {
        // Добавляем относительное положение цели (3 наблюдения)
        sensor.AddObservation(target.position - head.position);
    }
    */

    public override void OnEpisodeBegin()
    {
        // Перемещаем цель на случайную позицию
        target.position = randomTargetPosition();

        // Сбрасываем положение щупальца в начальную позицию
        foreach (var joint in joints)
        {
            joint.SetRotation(0);
        }
    }

    private Vector3 randomTargetPosition()
    {
        var point = Random.insideUnitSphere;
        point.Scale(targetSpawnScale);
        point += targetSpawnCenter;

        var vec2 = new Vector2(point.x, point.z);
        if (vec2.magnitude < targetCenterOffset)
        {
            return randomTargetPosition();
        }

        return transform.position + point;
    }

    public void OnDrawGizmosSelected()
    {
        if (!drawTargetGizmos) return;

        Gizmos.color = new Color(1, 0, 0, 0.75f);
        for (var i = 0; i < 100; i++)
        {
            Gizmos.DrawWireSphere(randomTargetPosition(), 0.1f);
        }
    }
}
