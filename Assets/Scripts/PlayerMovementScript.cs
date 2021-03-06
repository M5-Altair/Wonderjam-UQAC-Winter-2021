﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Net;
using System.Net.Sockets;
using System;


public class PlayerMovementScript : MonoBehaviour
{

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
    bool gameStart = true;
    AudioSource monsterSound;
    private bool musicFadeOutEnabled = false;
    AudioClip deathScream;
    AudioSource playerAudioSource;

    public string GetLocalIPv4()
    {
        IPAddress[] ipv4Addresses = Array.FindAll(
    Dns.GetHostEntry(string.Empty).AddressList,
    a => a.AddressFamily == AddressFamily.InterNetwork);
        return ipv4Addresses[0].ToString();
    }

    // Start is called before the first frame update
    void Start()
    {
        playerAudioSource = GetComponent<AudioSource>();
        deathScream = Resources.Load<AudioClip>("DeathScream");
        monsterSound = transform.Find("monsterSound").GetComponent<AudioSource>();

        ui.transform.Find("text").GetComponent<UnityEngine.UI.Text>().text += ("\n your ip : "+ GetLocalIPv4());

        movingBody = transform.Find("ScientistWalk").gameObject;
        animator = transform.Find("ScientistWalk").GetComponent<Animator>();
        nbProofFound = 0;
        body = GetComponent<Rigidbody>();

        for (int i = 0; i < 3; i++)
        {
            int randomChildIdx;
            Transform randomChild;
            do
            {
                randomChildIdx = UnityEngine.Random.Range(0, PossibleEvidences.transform.childCount);
                randomChild = PossibleEvidences.transform.GetChild(randomChildIdx);
            } while (camera2.GetComponent<cameraAnimation>().Evidences.FindIndex(d => d == randomChild.gameObject) != -1);
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
        if (Input.GetKey("space") && gameStart)
        { 
            ui.transform.Find("text").gameObject.SetActive(false);
            gameStart = false;
        }
        if (Input.GetKey("space") && atTheDoor && nbProofFound == 3)
        {
            Debug.Log("T TROP FORT FRERO");
            camera2.transform.Find("lost").gameObject.SetActive(true);
            ApplicationModel.ending = 1;
            SceneManager.LoadScene("EndGame");
        }
        if (Input.GetKey("space") && EvidenceInFront != null)
        {
            if (ui.transform.Find("loading").GetComponent<UnityEngine.UI.Slider>().value >= 100)
            {
                playerAudioSource.Stop();
                nbProofFound++;
                ui.transform.Find("evidences").GetComponent<UnityEngine.UI.Text>().text = "Evidences : "+ nbProofFound + " / 3";
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
                if (!playerAudioSource.isPlaying)
                    playerAudioSource.Play();
            }

        }
        else
        {
            playerAudioSource.Stop();
            ui.transform.Find("loading").gameObject.SetActive(false);
            ui.transform.Find("loading").GetComponent<UnityEngine.UI.Slider>().value = 0;
        }
        ui.transform.Find("LightEnergy").GetComponent<UnityEngine.UI.Slider>().value -= 0.35f/gameTimeMultiplier;
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

        if (musicFadeOutEnabled)
        {
            if (monsterSound.volume <= 0.1f)
            {
                monsterSound.Stop();
                musicFadeOutEnabled = false;
            }
            else
            {
                float newVolume = monsterSound.volume - (0.1f * Time.deltaTime);  //change 0.1f to something else to adjust the rate of the volume dropping
                if (newVolume < 0f)
                {
                    newVolume = 0f;
                }
                monsterSound.volume = newVolume;
            }
        }
    }

    private void FixedUpdate()
    {
        Vector3 normalizedDirection = new Vector3(horizontal, 0, vertical).normalized;
        body.velocity = normalizedDirection * speed;
        if (body.velocity != Vector3.zero)
        {
            animator.Play("Walking");
            movingBody.transform.rotation = Quaternion.LookRotation(body.velocity, Vector3.up);
        }
        else
        {
            animator.Play("Idle");
        }

    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "door")
        {
            atTheDoor = true;
            if (nbProofFound>2)
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
        else if (other.tag == "Monster")
        {
            if (!monsterSound.isPlaying)
            {
                musicFadeOutEnabled = false;
                monsterSound.volume = 1;
                monsterSound.Play();
                monsterSound.loop = true;
            }

        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Monster")
        {
            musicFadeOutEnabled = true;
        }
        else
        {
            ui.transform.Find("text").gameObject.SetActive(false);
            EvidenceInFront = null;
            atTheDoor = false;
        }
        

    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.tag == "Monster")
        {
            Debug.Log("PAN T MORT");
            camera2.transform.Find("lost").gameObject.SetActive(true);
            ApplicationModel.ending = 0;
            SceneManager.LoadScene("EndGame");
        }
    }
}
