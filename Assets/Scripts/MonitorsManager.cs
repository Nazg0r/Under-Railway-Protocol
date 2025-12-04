using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LocalEventManager))]
public class MonitorsManager : MonoBehaviour
{
    public LocalEventManager events; 
    public static MonitorsManager Instance { get; private set; }
    public int monitorsLength = 0;
    public float blockWidth = 1288f;

    public Color primaryColor = Color.black;
    public Color secondaryColor = new(172f / 255f, 16f / 255f, 16f / 255f);

    public class ShopProduct
    {
        public string id;
        public string title;
        public string description;
        public int available;
        public double pricePerOne;
    }

    public class QuestGoal
    {
        public string name;
        public int length;
    }

    public class QuestElement
    {
        public string id;
        public List<QuestGoal> goals;
    }

    public class Quest
    {
        public string station;
        public List<QuestElement> list;
    }

    public class Wagon
    {
        public string id;
        public string name;
        public int currentLoad;
        public int capacity;
    }

    public class Cargo
    {
        public string id;
        public string name;
        public int available;
    }

    public class Product
    {
        public string id;
        public int available;
    }

    public class PlayerData
    {
        public double wallet;
        public List<Product> products;
    }

    public PlayerData playerData = new()
    {
        wallet = 100,
        products = new List<Product>()
    };

    public List<ShopProduct> shopProducts = new();
    public List<Quest> quests = new();
    public List<Quest> acceptedQuests = new();
    public List<Wagon> wagons = new();
    public List<Cargo> cargos = new();

    private void GenerateShopItems()
    {
        string[] ids = new[] { "coal", "refrigirant", "power_cell", "regulator", "fuse" };
        string[] titles = new[] { "Coal", "Refrigirant", "Power Cell", "Regulator", "Fuse" };
        int[] availables = new[] { 10, 10, 10, 10, 10 };
        double[] prices = new[] { 0.5, 0.75, 1.0, 2.0, 1.5 };

        for (int index = 0; index < ids.Length; index++)
        {
            shopProducts.Add(new ShopProduct
            {
                id = ids[index],
                title = titles[index],
                description = "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor",
                available = availables[index],
                pricePerOne = prices[index]
            });
        }
    }

    private void GenerateQuests()
    {
        string[] ids = new[] { "quest_1", "quest_2", "quest_3", "quest_4", "quest_5" };
        string[] stations = new[] { "ST-100", "ST-200", "ST-300", "ST-400", "ST-500" };
        QuestGoal goal_1 = new QuestGoal
        {
            name = "Sigma-8",
            length = 10
        };

        QuestGoal goal_2 = new QuestGoal
        {
            name = "Protein",
            length = 100
        };

        for (int index = 0; index < stations.Length; index++)
        {
            quests.Add(new Quest
            {
                station = "→ " + stations[index],
                list = new List<QuestElement>
                {
                    new QuestElement
                    {
                        id = ids[index],
                        goals = new List<QuestGoal>
                        {
                            goal_1,
                            goal_2
                        }
                    },
                    new QuestElement
                    {
                        id = ids[index] + "_2",
                        goals = new List<QuestGoal>
                        {
                            goal_2,
                            goal_1
                        }
                    },
                    new QuestElement
                    {
                        id = ids[index] + "_3",
                        goals = new List<QuestGoal>
                        {
                            goal_1,
                            goal_2
                        }
                    }
                }
            });
        }
    }

    public void AcceptQuest(Quest quest, QuestElement questElement)
    {
        Quest item = acceptedQuests.Find(element => element.station == quest.station);
        if( item == null )
        {
            item = new Quest
            {
                station = quest.station,
                list = new()
            };

            item.list.Add(questElement);

            acceptedQuests.Add(item);
        } else
        {
            item.list.Add(questElement);
        }

        events.Invoke("UpdateAcceptedQuests");
    }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        events = GetComponent<LocalEventManager>();
        GenerateShopItems();
        GenerateQuests();
    }
}
