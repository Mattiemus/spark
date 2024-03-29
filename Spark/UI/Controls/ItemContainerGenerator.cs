﻿namespace Spark.UI.Controls
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;

    using Primitives;
    using Core.Collections;

    public sealed class ItemContainerGenerator : IRecyclingItemContainerGenerator
    {
        internal ItemContainerGenerator(ItemsControl owner)
        {
            Cache = new Queue<DependencyObject>();
            ContainerIndexMap = new DoubleKeyedDictionary<DependencyObject, int>();
            ContainerItemMap = new Dictionary<DependencyObject, object>();
            Owner = owner;
            RealizedElements = new RangeCollection();
        }

        public event ItemsChangedEventHandler ItemsChanged;

        internal GenerationState GenerationState { get; set; }

        private RangeCollection RealizedElements { get; set; }

        private Queue<DependencyObject> Cache { get; set; }

        private DoubleKeyedDictionary<DependencyObject, int> ContainerIndexMap { get; set; }

        private Dictionary<DependencyObject, object> ContainerItemMap { get; set; }

        private ItemsControl Owner { get; set; }

        private Panel Panel => Owner.Panel;

        public DependencyObject ContainerFromIndex(int index)
        {
            DependencyObject container;
            ContainerIndexMap.TryMap(index, out container);
            return container;
        }

        public DependencyObject ContainerFromItem(object item)
        {
            if (item != null)
            {
                foreach (var v in ContainerItemMap)
                {
                    if (object.Equals(v.Value, item))
                    {
                        return v.Key;
                    }
                }
            }

            return null;
        }

        public GeneratorPosition GeneratorPositionFromIndex(int itemIndex)
        {
            if (itemIndex < 0)
            {
                return new GeneratorPosition(-1, 0);
            }

            if (RealizedElements.Contains(itemIndex))
            {
                return new GeneratorPosition(RealizedElements.IndexOf(itemIndex), 0);
            }

            if (itemIndex > Owner.Items.Count)
            {
                return new GeneratorPosition(-1, 0);
            }

            if (RealizedElements.Count == 0)
            {
                return new GeneratorPosition(-1, itemIndex + 1);
            }

            int index = -1;

            for (int i = 0; i < RealizedElements.Count; i++)
            {
                if (RealizedElements[i] > itemIndex)
                {
                    break;
                }

                index = i;
            }

            if (index == -1)
            {
                return new GeneratorPosition(index, itemIndex + 1);
            }

            return new GeneratorPosition(index, itemIndex - RealizedElements[index]);
        }

        public int IndexFromContainer(DependencyObject container)
        {
            int index;
            if (!ContainerIndexMap.TryMap(container, out index))
            {
                index = -1;
            }

            return index;
        }

        public int IndexFromGeneratorPosition(GeneratorPosition position)
        {
            // We either have everything realised or nothing realised, so we can
            // simply just add Index and Offset together to get the right index (i think)
            if (position.Index == -1)
            {
                if (position.Offset < 0)
                {
                    return Owner.Items.Count + position.Offset;
                }

                return position.Offset - 1;
            }

            if (position.Index > Owner.Items.Count)
            {
                return -1;
            }

            if (position.Index >= 0 && position.Index < RealizedElements.Count)
            {
                return RealizedElements[position.Index] + position.Offset;
            }

            return position.Index + position.Offset;
        }

        public object ItemFromContainer(DependencyObject container)
        {
            object item;
            ContainerItemMap.TryGetValue(container, out item);
            return item ?? DependencyProperty.UnsetValue;
        }

        DependencyObject IItemContainerGenerator.GenerateNext(out bool isNewlyRealized)
        {
            return GenerateNext(out isNewlyRealized);
        }

        ItemContainerGenerator IItemContainerGenerator.GetItemContainerGeneratorForPanel(Panel panel)
        {
            return GetItemContainerGeneratorForPanel(panel);
        }

        void IItemContainerGenerator.PrepareItemContainer(DependencyObject container)
        {
            PrepareItemContainer(container);
        }

        void IItemContainerGenerator.Remove(GeneratorPosition position, int count)
        {
            Remove(position, count);
        }

        void IItemContainerGenerator.RemoveAll()
        {
            RemoveAll();
        }

        IDisposable IItemContainerGenerator.StartAt(
            GeneratorPosition position,
            GeneratorDirection direction,
            bool allowStartAtRealizedItem)
        {
            return StartAt(position, direction, allowStartAtRealizedItem);
        }

        void IRecyclingItemContainerGenerator.Recycle(GeneratorPosition position, int count)
        {
            Recycle(position, count);
        }

        internal DependencyObject GenerateNext(out bool isNewlyRealized)
        {
            if (GenerationState == null)
            {
                throw new InvalidOperationException("Cannot call GenerateNext before calling StartAt");
            }

            int index;

            // This is relative to the realised elements.
            int startAt = GenerationState.Position.Index;

            if (startAt == -1)
            {
                if (GenerationState.Position.Offset < 0)
                {
                    index = Owner.Items.Count + GenerationState.Position.Offset;
                }
                else if (GenerationState.Position.Offset == 0)
                {
                    index = 0;
                }
                else
                {
                    index = GenerationState.Position.Offset - 1;
                }
            }
            else if (startAt >= 0 && startAt < RealizedElements.Count)
            {
                // We're starting relative to an already realised element
                index = RealizedElements[startAt] + GenerationState.Position.Offset;
            }
            else
            {
                index = -1;
            }

            bool alreadyRealized = RealizedElements.Contains(index);

            if (!GenerationState.AllowStartAtRealizedItem && alreadyRealized && GenerationState.Position.Offset == 0)
            {
                index += GenerationState.Step;
                alreadyRealized = RealizedElements.Contains(index);
            }

            if (index < 0 || index >= Owner.Items.Count)
            {
                isNewlyRealized = false;
                return null;
            }

            if (alreadyRealized)
            {
                GenerationState.Position = new GeneratorPosition(RealizedElements.IndexOf(index), GenerationState.Step);
                isNewlyRealized = false;

                return ContainerIndexMap[index];
            }

            DependencyObject container;
            object item = Owner.Items[index];

            if (Owner.IsItemItsOwnContainer(item))
            {
                container = (DependencyObject)item;
                isNewlyRealized = true;
            }
            else
            {
                if (Cache.Count == 0)
                {
                    container = Owner.GetContainerForItem();
                    isNewlyRealized = true;
                }
                else
                {
                    container = Cache.Dequeue();
                    isNewlyRealized = false;
                }
            }

            FrameworkElement f = container as FrameworkElement;

            if (f != null && !(item is UIElement))
            {
                f.DataContext = item;
            }

            RealizedElements.Add(index);
            ContainerIndexMap.Add(container, index);
            ContainerItemMap.Add(container, item);

            GenerationState.Position = new GeneratorPosition(RealizedElements.IndexOf(index), GenerationState.Step);

            return container;
        }

        internal ItemContainerGenerator GetItemContainerGeneratorForPanel(Panel panel)
        {
            // FIXME: Double check this, but i think it's right
            return panel == Panel ? this : null;
        }

        internal void OnOwnerItemsItemsChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            int itemCount;
            int itemUICount;
            GeneratorPosition oldPosition = new GeneratorPosition(-1, 0);
            GeneratorPosition position;

            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    if ((e.NewStartingIndex + 1) != Owner.Items.Count)
                    {
                        MoveExistingItems(e.NewStartingIndex, 1);
                    }

                    itemCount = 1;
                    itemUICount = 0;
                    position = GeneratorPositionFromIndex(e.NewStartingIndex);
                    break;

                case NotifyCollectionChangedAction.Remove:
                    itemCount = 1;
                    itemUICount = RealizedElements.Contains(e.OldStartingIndex) ? 1 : 0;
                    position = GeneratorPositionFromIndex(e.OldStartingIndex);

                    if (itemUICount == 1)
                    {
                        Remove(position, 1);
                    }

                    MoveExistingItems(e.OldStartingIndex, -1);
                    break;

                case NotifyCollectionChangedAction.Replace:
                    if (!RealizedElements.Contains(e.NewStartingIndex))
                    {
                        return;
                    }

                    itemCount = 1;
                    itemUICount = 1;
                    position = GeneratorPositionFromIndex(e.NewStartingIndex);

                    Remove(position, 1);

                    bool fresh;
                    var newPos = GeneratorPositionFromIndex(e.NewStartingIndex);
                    using (StartAt(newPos, GeneratorDirection.Forward, true))
                    {
                        PrepareItemContainer(GenerateNext(out fresh));
                    }

                    break;

                case NotifyCollectionChangedAction.Reset:
                    itemCount = e.OldItems == null ? 0 : e.OldItems.Count;
                    itemUICount = RealizedElements.Count;
                    position = new GeneratorPosition(-1, 0);
                    RemoveAll();
                    break;

                default:
                    Console.WriteLine("*** Critical error in ItemContainerGenerator.OnOwnerItemsItemsChanged. NotifyCollectionChangedAction.{0} is not supported", e.Action);
                    return;
            }

            ItemsChangedEventArgs args = new ItemsChangedEventArgs
            {
                Action = e.Action,
                ItemCount = itemCount,
                ItemUICount = itemUICount,
                OldPosition = oldPosition,
                Position = position
            };

            ItemsChanged?.Invoke(this, args);
        }

        internal void PrepareItemContainer(DependencyObject container)
        {
            var index = ContainerIndexMap[container];
            var item = Owner.Items[index];

            Owner.PrepareContainerForItem(container, item);
        }

        internal void Remove(GeneratorPosition position, int count)
        {
            CheckOffsetAndRealized(position, count);

            int index = IndexFromGeneratorPosition(position);
            for (int i = 0; i < count; i++)
            {
                var container = ContainerIndexMap[index + i];
                var item = ContainerItemMap[container];
                ContainerIndexMap.Remove(container, index + i);
                ContainerItemMap.Remove(container);
                RealizedElements.Remove(index + i);
                Owner.ClearContainerForItem(container, item);
            }
        }

        internal IDisposable StartAt(
            GeneratorPosition position,
            GeneratorDirection direction,
            bool allowStartAtRealizedItem)
        {
            if (GenerationState != null)
            {
                throw new InvalidOperationException("Cannot call StartAt while a generation operation is in progress");
            }

            GenerationState = new GenerationState
            {
                AllowStartAtRealizedItem = allowStartAtRealizedItem,
                Direction = direction,
                Position = position,
                Generator = this
            };

            return GenerationState;
        }

        internal void Recycle(GeneratorPosition position, int count)
        {
            CheckOffsetAndRealized(position, count);

            int index = IndexFromGeneratorPosition(position);

            for (int i = 0; i < count; i++)
            {
                Cache.Enqueue(ContainerIndexMap[index + i]);
            }

            Remove(position, count);
        }

        internal void RemoveAll()
        {
            foreach (var pair in ContainerItemMap)
            {
                Owner.ClearContainerForItem(pair.Key, pair.Value);
            }

            RealizedElements.Clear();
            ContainerIndexMap.Clear();
            ContainerItemMap.Clear();
        }

        private void CheckOffsetAndRealized(GeneratorPosition position, int count)
        {
            if (position.Offset != 0)
            {
                throw new ArgumentException("position.Offset must be zero as the position must refer to a realized element.");
            }

            int index = IndexFromGeneratorPosition(position);
            int rangeIndex = RealizedElements.FindRangeIndexForValue(index);
            RangeCollection.Range range = RealizedElements.Ranges[rangeIndex];

            if (index < range.Start || (index + count) > range.Start + range.Count)
            {
                throw new InvalidOperationException("Only items which have been Realized can be removed.");
            }
        }

        private void MoveExistingItems(int index, int offset)
        {
            // This is a little horrible. I should really collapse the existing
            // RangeCollection so that every > the current index is decremented by 1.
            // This is easier for now though. I may think of a better way later on.
            RangeCollection newRanges = new RangeCollection();
            List<int> list = new List<int>();

            for (int i = 0; i < RealizedElements.Count; i++)
            {
                list.Add(RealizedElements[i]);
            }

            if (offset > 0)
            {
                list.Reverse();
            }

            foreach (int i in list)
            {
                int oldIndex = i;

                if (oldIndex < index)
                {
                    newRanges.Add(oldIndex);
                }
                else
                {
                    newRanges.Add(oldIndex + offset);
                    var container = ContainerIndexMap[oldIndex];
                    ContainerIndexMap.Remove(container, oldIndex);
                    ContainerIndexMap.Add(container, oldIndex + offset);
                }
            }

            RealizedElements = newRanges;
        }
    }
}
