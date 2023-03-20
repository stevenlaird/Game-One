using System.Collections;
using UnityEngine;

public class TileHealth : MonoBehaviour
{

    public int tileHealth;
    private int tileStartingHealth;
    public int timeToDestroy;

    public Sprite damageSprites1;
    public Sprite damageSprites2;
    public Sprite damageSprites3;
    public Sprite damageSprites4;
    public Sprite damageSprites5;
    public Sprite damageSprites6;
    public Sprite damageSprites7;
    public Sprite damageSprites8;
    public Sprite damageSprites9;
    //public Sprite damageSprites10;

    ///////////////////

    void Start()
    {
        tileStartingHealth = tileHealth;

        damageSprites1 = Resources.Load<Sprite>("TileDamage_1");
        damageSprites2 = Resources.Load<Sprite>("TileDamage_2");
        damageSprites3 = Resources.Load<Sprite>("TileDamage_3");
        damageSprites4 = Resources.Load<Sprite>("TileDamage_4");
        damageSprites5 = Resources.Load<Sprite>("TileDamage_5");
        damageSprites6 = Resources.Load<Sprite>("TileDamage_6");
        damageSprites7 = Resources.Load<Sprite>("TileDamage_7");
        damageSprites8 = Resources.Load<Sprite>("TileDamage_8");
        damageSprites9 = Resources.Load<Sprite>("TileDamage_9");
    }

    void Update()
    {
        for (int i = 0; i < this.transform.childCount; i++)
        {
            if (this.transform.GetChild(i).gameObject.name == "TileDamage(Clone)")
            {
                if (tileHealth <= tileStartingHealth * .90 && tileHealth > tileStartingHealth * .80)
                { this.transform.GetChild(i).GetComponent<SpriteRenderer>().sprite = damageSprites1; }
                if (tileHealth <= tileStartingHealth * .80 && tileHealth > tileStartingHealth * .70)
                { this.transform.GetChild(i).GetComponent<SpriteRenderer>().sprite = damageSprites2; }
                if (tileHealth <= tileStartingHealth * .70 && tileHealth > tileStartingHealth * .60)
                { this.transform.GetChild(i).GetComponent<SpriteRenderer>().sprite = damageSprites3; }
                if (tileHealth <= tileStartingHealth * .60 && tileHealth > tileStartingHealth * .50)
                { this.transform.GetChild(i).GetComponent<SpriteRenderer>().sprite = damageSprites4; }
                if (tileHealth <= tileStartingHealth * .50 && tileHealth > tileStartingHealth * .40)
                { this.transform.GetChild(i).GetComponent<SpriteRenderer>().sprite = damageSprites5; }
                if (tileHealth <= tileStartingHealth * .40 && tileHealth > tileStartingHealth * .30)
                { this.transform.GetChild(i).GetComponent<SpriteRenderer>().sprite = damageSprites6; }
                if (tileHealth <= tileStartingHealth * .30 && tileHealth > tileStartingHealth * .20)
                { this.transform.GetChild(i).GetComponent<SpriteRenderer>().sprite = damageSprites7; }
                if (tileHealth <= tileStartingHealth * .20 && tileHealth > tileStartingHealth * .10)
                { this.transform.GetChild(i).GetComponent<SpriteRenderer>().sprite = damageSprites8; }
                if (tileHealth <= tileStartingHealth * .10 && tileHealth > 0)
                { this.transform.GetChild(i).GetComponent<SpriteRenderer>().sprite = damageSprites9; }

                if (this.transform.name == "Tree Log" || this.transform.name == "Tree Base")
                {
                    this.transform.GetChild(i).localScale = new Vector3(0.65f, 1, 1);
                }

                if (this.name == "Crafting Table")
                {
                    this.transform.GetChild(i).localPosition = new Vector2(0.5f, 0.5f);
                    this.transform.GetChild(i).localScale = new Vector3(2, 2, 1);
                }
            }
        }
    }
}
