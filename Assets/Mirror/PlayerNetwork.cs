﻿using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerNetwork : NetworkBehaviour
{
    GameObject gameParts;
    [SyncVar] public bool switchViews = false;

    //this will sync var from server to all clients by calling the "SyncVarWithClientsRpc" funtion on the clients with the value of the variable "varToSync" equals to the value of "example1"
    /*[ClientRpc]
    public void RpcSyncVarWithClients(bool varToSync)
    {
        switchViews = varToSync;
    }*/

    float horizontal;
    float vertical;
    Rigidbody body;
    public Canvas ui;
    public float speed = 5.0f;
    public int nbProofFound;
    public GameObject PossibleEvidences;
    public Canvas camera2;
    public float gameTimeMultiplier = 1;
    Animator animator;
    GameObject movingBody;
    GameObject EvidenceInFront = null;
    bool atTheDoor = false;

    // Start is called before the first frame update
    void Start()
    {


        gameParts = GameObject.FindGameObjectWithTag("GameParts");
        movingBody = transform.Find("ScientistWalk").gameObject;
        animator = transform.Find("ScientistWalk").GetComponent<Animator>();
        nbProofFound = 0;
        body = GetComponent<Rigidbody>();

        for (int i = 0; i < 3; i++)
        {
            int randomChildIdx = Random.Range(0, PossibleEvidences.transform.childCount);
            Transform randomChild = PossibleEvidences.transform.GetChild(randomChildIdx);
            Vector3 collidSize = randomChild.GetComponent<BoxCollider>().size;
            randomChild.GetComponent<BoxCollider>().size = new Vector3(collidSize.x + 2, collidSize.y, collidSize.z + 2);
            randomChild.tag = "evidence";
            //Debug.Log(camera2.GetComponent<cameraAnimation>().Evidences);
            camera2.GetComponent<cameraAnimation>().Evidences.Add(randomChild.gameObject);
        }
    }

    // Update is called once per frame
    void Update()
    {
        horizontal = Input.GetAxis("Horizontal");
        vertical = Input.GetAxis("Vertical");
        if (Input.GetKey("space") && atTheDoor && nbProofFound == 3)
        {
            Debug.Log("T TROP FORT FRERO");
            ApplicationModel.ending = 1;
            SceneManager.LoadScene("EndGame");
        }
        if (Input.GetKey("space") && EvidenceInFront != null)
        {
            if (ui.transform.Find("loading").GetComponent<UnityEngine.UI.Slider>().value >= 100)
            {
                nbProofFound++;
                ui.transform.Find("evidences").GetComponent<UnityEngine.UI.Text>().text = "Evidences : " + nbProofFound + " / 3";
                EvidenceInFront.tag = "Untagged";
                ui.transform.Find("text").gameObject.SetActive(false);
                int index = camera2.GetComponent<cameraAnimation>().Evidences.FindIndex(d => d == EvidenceInFront.gameObject);
                camera2.transform.Find("evidence" + index).gameObject.SetActive(false);
                EvidenceInFront = null;
            }
            else
            {
                ui.transform.Find("loading").gameObject.SetActive(true);
                ui.transform.Find("loading").GetComponent<UnityEngine.UI.Slider>().value += 0.3f;
            }

        }
        else
        {
            ui.transform.Find("loading").gameObject.SetActive(false);
            ui.transform.Find("loading").GetComponent<UnityEngine.UI.Slider>().value = 0;
        }
        ui.transform.Find("LightEnergy").GetComponent<UnityEngine.UI.Slider>().value -= 0.35f / gameTimeMultiplier;
        foreach (GameObject light in GameObject.FindGameObjectsWithTag("Light"))
        {
            light.GetComponent<Light>().range = 15 + ui.transform.Find("LightEnergy").GetComponent<UnityEngine.UI.Slider>().value * 0.35f;
        }
        if (ui.transform.Find("LightEnergy").GetComponent<UnityEngine.UI.Slider>().value == 0)
        {
            Debug.Log("PAN T MORT");
            ApplicationModel.ending = 0;
            SceneManager.LoadScene("EndGame");
        }

        if (switchViews == true)
        {
            if (isLocalPlayer)
            {
                gameParts.transform.Find("MapPart").gameObject.SetActive(true);
                gameParts.transform.Find("GamePart").gameObject.SetActive(true);
                gameParts.transform.Find("LobbyPart").Find("Canvas").gameObject.SetActive(false);
                gameParts.transform.Find("LobbyPart").Find("Main Camera").gameObject.SetActive(false);
                gameParts.transform.Find("GamePart").Find("Player").Find("Camera").gameObject.SetActive(false);
                gameParts.transform.Find("GamePart").Find("CanvasPlayer1").gameObject.SetActive(false);
            }
            else
            {
                gameParts.transform.Find("MapPart").gameObject.SetActive(true);
                gameParts.transform.Find("GamePart").gameObject.SetActive(true);
                gameParts.transform.Find("LobbyPart").Find("Canvas").gameObject.SetActive(false);
                gameParts.transform.Find("LobbyPart").Find("Main Camera").gameObject.SetActive(false);
                gameParts.transform.Find("MapPart").Find("CameraPlayer2").gameObject.SetActive(false);
                gameParts.transform.Find("MapPart").Find("CanvasPlayer2").gameObject.SetActive(false);
            }

        }



    }

    private void FixedUpdate()
    {
        Vector3 normalizedDirection = new Vector3(horizontal, 0, vertical).normalized;
        body.velocity = normalizedDirection * speed;
        if (body.velocity != Vector3.zero)
        {
            if (animator.speed == 0)
                animator.speed = 1;
            movingBody.transform.rotation = Quaternion.LookRotation(body.velocity, Vector3.up);
        }
        else
        {
            animator.speed = 0;
        }

    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "door")
        {
            atTheDoor = true;
            if (nbProofFound > 2)
            {
                ui.transform.Find("text").GetComponent<UnityEngine.UI.Text>().text = "press space to exit";
            }
            else
            {
                ui.transform.Find("text").GetComponent<UnityEngine.UI.Text>().text = "not enough evidences to exit";
            }
            ui.transform.Find("text").gameObject.SetActive(true);
        }
        else if (other.tag == "evidence")
        {
            EvidenceInFront = other.gameObject;
            ui.transform.Find("text").GetComponent<UnityEngine.UI.Text>().text = "press space to search for evidence";
            ui.transform.Find("text").gameObject.SetActive(true);
        }
    }
    private void OnTriggerExit(Collider other)
    {
        ui.transform.Find("text").gameObject.SetActive(false);
        EvidenceInFront = null;
        atTheDoor = false;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.tag == "Monster")
        {
            Debug.Log("PAN T MORT");
            ApplicationModel.ending = 0;
            SceneManager.LoadScene("EndGame");
        }
    }


    private void OnGUI()
    {

        GUI.Label(new Rect(10, 10, 100, 20), switchViews.ToString());

    }


    public override void OnStartClient()
    {
        print("starting client");
    }

    public override void OnStopClient()
    {
        print("stopping client");
    }

    public override void OnStartServer()
    {
        print("starting server");
    }

    public override void OnStopServer()
    {
        print("stopping server");
    }
}
