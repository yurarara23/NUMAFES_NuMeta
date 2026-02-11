
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class kotatsu : UdonSharpBehaviour
{
    public GameObject homePosition;
    
    public GameObject kota1;
    public GameObject kota2;
    public GameObject tata1;
    public GameObject tata2;
    public GameObject[] zabu1_4;

    [Range(0f, 4f)] public float speed = 2f;

    private float time_cloth = 0f;
    private float cloth = 0f;
    private float clothS = 0f;
    private float time_kota = 0f;
    private float kota = 0f;
    private float time_tata = 0f;
    private float tata = 0f;

    private int[] zabu_on = { 0, 0, 0, 0 };

    private SkinnedMeshRenderer skinnedMeshRenderer;
    
    void Start()
    {
        skinnedMeshRenderer = kota2.GetComponent<SkinnedMeshRenderer>();
    }

    void Update()
    {
        Vector3 pos_home = homePosition.transform.position;
        Vector3 sca_home = homePosition.transform.localScale;

        if (!kota1.activeSelf)
        {
            tata1.transform.position = pos_home;
            tata2.transform.position = pos_home;
            kota1.transform.position = pos_home + new Vector3(0f, -sca_home.y, 0f);

            kota2.transform.localScale = new Vector3(0.4f, 1f, 0.4f);
            skinnedMeshRenderer.SetBlendShapeWeight(0, 0f);

            cloth = 0f;
            time_cloth = 0f;
            kota = 0f;
            time_kota = 0f;
            tata = 0f;
            time_tata = 0f;

            for (int i = 1; i <= 4; i++)
            {
                skinnedMeshRenderer.SetBlendShapeWeight(i, 0);
            }
        }

        if (kota1.activeSelf)
        {
            if (tata != 2f)
            {
                Vector3 pos_tata1 = tata1.transform.position;
                Vector3 pos_tata2 = tata2.transform.position;
                
                time_tata += speed * Time.deltaTime;
                tata = 1f - Mathf.Cos(time_tata);

                if (tata >= 1.9f)
                {
                    tata = 2f;
                }

                if (tata < 1f)
                {
                    pos_tata1.y = -0.1f * tata * sca_home.y + pos_home.y;
                    pos_tata2.y = -0.1f * tata * sca_home.y + pos_home.y;
                }
                else
                {
                    pos_tata1.z = (1f - tata) * sca_home.z + pos_home.z;
                    pos_tata2.z = (tata - 1f) * sca_home.z + pos_home.z;
                }

                tata1.transform.position = pos_tata1;
                tata2.transform.position = pos_tata2;
            }

            if (kota != 2f && tata == 2f)
            {
                Vector3 pos_kota1 = kota1.transform.position;
                Vector3 pos_kota2 = kota2.transform.position;
                
                time_kota += speed * Time.deltaTime;
                kota = 1f - Mathf.Cos(time_kota);

                if (kota >= 1.9f)
                {
                    kota = 2f;
                }

                pos_kota1.y = (0.5f * kota - 1f) * sca_home.y + pos_home.y;

                kota1.transform.position = pos_kota1;
            }

            if (cloth != 2f && kota == 2f)
            {
                time_cloth += speed * Time.deltaTime;
                cloth = 1f - Mathf.Cos(time_cloth);

                if (cloth >= 1.9f)
                {
                    cloth = 2f;
                }

                clothS = 0.3f * cloth + 0.4f;

                skinnedMeshRenderer.SetBlendShapeWeight(0, 50f * cloth);
                for (int i = 0; i < 4; i++)
                {
                    if (zabu1_4[i].activeSelf)
                    {
                        zabu_on[i] = 1;
                    }
                    else
                    {
                        zabu_on[i] = 0;
                    }

                    skinnedMeshRenderer.SetBlendShapeWeight(i + 1, zabu_on[i] * 50f * cloth);
                }

                kota2.transform.localScale = new Vector3(clothS, 1f, clothS);
            }

            if (cloth == 2f)
            {
                for (int i = 0; i < 4; i++)
                {
                    if (zabu1_4[i].activeSelf)
                    {
                        zabu_on[i] = 1;
                    }
                    else
                    {
                        zabu_on[i] = 0;
                    }

                    skinnedMeshRenderer.SetBlendShapeWeight(i + 1, zabu_on[i] * 100);
                }
            }
        }
    }
}
