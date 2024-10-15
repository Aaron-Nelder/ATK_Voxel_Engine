using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PhysicsSimulation : MonoBehaviour
{
    public static PhysicsSimulation Instance;
    private Scene simulationScene;
    [SerializeField] SimulationMode simulationMode = SimulationMode.FixedUpdate;
    public SimulationMode SimulationMode => simulationMode;
    private PhysicsScene physicsScene;
    [Range(1, 100)]
    [SerializeField] float stepMultiplier = 1;
    public float StepMultiplier => stepMultiplier;
    [SerializeField] Transform physicsObjects;

    void Awake()
    {
        Instance = this;
        CreatePhysicsScene();
    }

    public void CreatePhysicsScene()
    {
        simulationScene = SceneManager.CreateScene("PhysicsSimulation", new CreateSceneParameters(LocalPhysicsMode.Physics3D));
        physicsScene = simulationScene.GetPhysicsScene();

        foreach (Transform t in physicsObjects)
        {
            var go = Instantiate(t.gameObject, t.position, t.rotation);
            SceneManager.MoveGameObjectToScene(go, simulationScene);
        }
    }
    
    public void SimulateForce(GameObject objectToSimulate, float force, Vector3 direction, float simulationTime, out List<Vector3> positions)
    {
        GameObject spawnedObj = Instantiate(objectToSimulate.gameObject, objectToSimulate.transform.position, objectToSimulate.transform.rotation);

        SceneManager.MoveGameObjectToScene(spawnedObj, simulationScene);

        spawnedObj.GetComponent<Collider>().enabled = true;
        Rigidbody rb = spawnedObj.GetComponent<Rigidbody>();
        rb.isKinematic = false;
        rb.useGravity = true;
        rb.AddForce(direction * force, ForceMode.Impulse);

        positions = new List<Vector3>();

        float time = GetSimulationTime(simulationMode);

        for (float i = 0; i < simulationTime; i += stepMultiplier * time)
        {
            positions.Add(spawnedObj.transform.position);
            physicsScene.Simulate(stepMultiplier * time);
        }
        Destroy(spawnedObj);
    }

    public float GetSimulationTime(SimulationMode mode)
    {
        if (mode == SimulationMode.FixedUpdate)
            return Time.fixedDeltaTime;
        else
            return Time.deltaTime;
    }
}

public struct SimulationData
{
    public Vector3 position;
    public Quaternion rotation;
    public float time;
    public Vector3 relativePosition;
    public Vector3 direction;

    public SimulationData(Vector3 pos, Vector3 relativePos, Vector3 dir, Quaternion rot, float t)
    {
        position = pos;
        relativePosition = relativePos;
        direction = dir;
        rotation = rot;
        time = t;
    }
}
