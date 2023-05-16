using UnityEngine;

public class TileHealth : MonoBehaviour
{
    // Current health of the tile, assigned by TerrainGeneration using the corresponding TileClass
    public int tileHealth;
    // Stored value for starting health of the tile
    private int tileStartingHealth;

    // Sprites representing the tile's damage state
    public Sprite damageSprites1;   // Sprite for 10% Damage
    public Sprite damageSprites2;   // Sprite for 20% Damage
    public Sprite damageSprites3;   // Sprite for 30% Damage
    public Sprite damageSprites4;   // Sprite for 40% Damage
    public Sprite damageSprites5;   // Sprite for 50% Damage
    public Sprite damageSprites6;   // Sprite for 60% Damage
    public Sprite damageSprites7;   // Sprite for 70% Damage
    public Sprite damageSprites8;   // Sprite for 80% Damage
    public Sprite damageSprites9;   // Sprite for 90% Damage

    ///////////////////

    // Initialize the starting health of the tile, and load the sprites needed in Update()
    void Start()
    {
        // Initialize starting health
        tileStartingHealth = tileHealth;
        // Load Sprites from 'Sprites/Resources' for each level of damage
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

    // Update the Sprite of the tile's child GameObject to indicate the amount of damage that has been done to the tile
    void Update()
    {
        // Loop through all the children of the tile
        for (int i = 0; i < this.transform.childCount; i++)
        {
            // Check if the child GameObject is the damage indicating child GameObject
            if (this.transform.GetChild(i).gameObject.name == "TileDamage(Clone)")
            {
                // IF tile health is between X% and X% of starting health
                // Update the Sprite of the damage indiciating child GameObject to reflect the tile's current health
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

                // IF tile is a Tree Log or Tree Log Top or Tree Base
                if (this.transform.name.Contains("Tree Log") || this.transform.name.Contains("Tree Log Top") || this.transform.name.Contains("Tree Base"))
                {
                    // Resize the width of the damage indicating Sprite to fit the width of the Sprite used by trees
                    this.transform.GetChild(i).localScale = new Vector3(0.65f, 1, 1);
                }

                // IF tile is a Crafting Table
                if (this.name == "Crafting Table")
                {
                    // Move and resize the damage indicating Sprite to fit the larger sized crafting table tiles
                    this.transform.GetChild(i).localPosition = new Vector2(0.5f, 0.5f);
                    this.transform.GetChild(i).localScale = new Vector3(2, 2, 1);
                }
            }
        }
    }
}