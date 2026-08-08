using UnityEngine;


/// <summary>
/// Необходим для присваивания уникального ID всем экземплярам растений, батареек и ферментов. Используется для сохранений забега(RunData)
/// </summary>
public abstract class UniqueScriptableObject : ScriptableObject
{
    [SerializeField] private string _id; 

    public string Id
    {
        get
        {
            // Если ID ещё не задан, генерируем (только в редакторе)
#if UNITY_EDITOR
            if (string.IsNullOrEmpty(_id))
            {
                GenerateId();
                UnityEditor.EditorUtility.SetDirty(this);
            }
#endif
            return _id;
        }
    }

    /// <summary>
    /// Генерирует новый уникальный ID. Вызывается при создании ассета.
    /// </summary>
    protected void GenerateId()
    {
        _id = System.Guid.NewGuid().ToString();
#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
        UnityEditor.AssetDatabase.SaveAssets();
#endif
    }

    // Опционально: метод для ручной генерации (если нужно перегенерировать)
#if UNITY_EDITOR
    public void RegenerateId()
    {
        GenerateId();
    }
#endif
}