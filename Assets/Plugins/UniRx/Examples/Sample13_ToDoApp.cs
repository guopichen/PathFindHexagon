// for uGUI(from 4.6)
#if !(UNITY_4_0 || UNITY_4_1 || UNITY_4_2 || UNITY_4_3 || UNITY_4_4 || UNITY_4_5)

using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.EventSystems;

namespace UniRx.Examples
{
    public class Sample13_ToDoApp : MonoBehaviour
    {
        // Open Sample13Scene. Set from canvas
        public Text Title;
        public InputField ToDoInput;
        public Button AddButton;
        public Button ClearButton;
        public GameObject TodoList;

        // prefab:)
        public GameObject SampleItemPrefab;

        ReactiveCollection<GameObject> toDos = new ReactiveCollection<GameObject>();

        void Start()
        {
            //将可能产生提交stream的 stream 合并为一个 stream

            // merge Button click and push enter key on input field.
            var submit = Observable.Merge(
                AddButton.OnClickAsObservable().Select(_ => ToDoInput.text),
                ToDoInput.OnEndEditAsObservable().Where(_ => Input.GetKeyDown(KeyCode.Return)));

            //对上述合并的stream进行过滤得到（我们感兴趣的）非空的stream流
            //同时对我们感兴趣的stream添加处理操作
            //将引发数据变动，（新的stream）
            // add to reactive collection
            submit.Where(x => x != "")
                  .Subscribe(x =>
                  {
                      ToDoInput.text = ""; // clear input field
                      var item = Instantiate(SampleItemPrefab) as GameObject;
                      (item.GetComponentInChildren(typeof(Text)) as Text).text = x;
                      toDos.Add(item);
                  });

            //上面新的stream 变动
            // Collection Change Handling
            toDos.ObserveCountChanged().Subscribe(x => Title.text = "TODO App, ItemCount:" + x);
            toDos.ObserveAdd().Subscribe(x =>
            {
                x.Value.transform.SetParent(TodoList.transform, false);
            });
            toDos.ObserveRemove().Subscribe(x =>
            {
                GameObject.Destroy(x.Value);
            });

            // Clear
            ClearButton.onClick.AsObservable()
                .Subscribe(_ =>
                {
                    var removeTargets = toDos.Where(x => x.GetComponent<Toggle>().isOn).ToArray();
                    foreach (var item in removeTargets)
                    {
                        toDos.Remove(item);
                    }
                });
        }
    }
}

#endif