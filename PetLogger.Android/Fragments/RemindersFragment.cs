﻿using Android.OS;
using Android.Support.Design.Widget;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using PetLogger.Droid.Adapters;
using PetLogger.Droid.Components;
using PetLogger.Droid.Helpers;
using PetLogger.Shared.Data;
using PetLogger.Shared.DataAccessLayer;
using Fragment = Android.Support.V4.App.Fragment;

namespace PetLogger.Droid.Fragments
{
    public class RemindersFragment : Fragment, /*ISearchFragment, */AbsListView.IMultiChoiceModeListener
    {
        public static RemindersFragment Instantiate() => new RemindersFragment();

        private ReminderAdapter _reminderAdapter;

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState) => inflater.Inflate(Resource.Layout.fragment_reminders, container, false);

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            base.OnViewCreated(view, savedInstanceState);

            ToolbarHelper.ShowToolbar(Activity, "Reminders");
            ToolbarHelper.HideToolbarBackButton(Activity);

            var reminderRecycler = view.FindViewById<RecyclerView>(Resource.Id.reminder_recycler);
            SetUpReminderAdapter(reminderRecycler, view);

            var fabAddReminder = view.FindViewById<FloatingActionButton>(Resource.Id.fab_add_reminder);
            fabAddReminder.Click += (s, args) => FragmentHelper.Add(Activity, AddEntityFragment<Reminder>.Instantiate("Reminder"));
        }

        private void SetUpReminderAdapter(RecyclerView recyclerView, View view)
        {
            ((SimpleItemAnimator)recyclerView.GetItemAnimator()).SupportsChangeAnimations = false;
            recyclerView.SetLayoutManager(new LinearLayoutManager(Context));
            recyclerView.AddItemDecoration(new VerticalSpaceItemDecoration(40));

            _reminderAdapter = new ReminderAdapter(Activity, DBTable.GetAll<Reminder>().ToList());
            _reminderAdapter.SetMultiChoiceModeListener(this);
            //_reminderAdapter.ItemClick += (s, args) => FragmentHelper.Add(Activity, IncidentDetailFragment.Instantiate(args.Item.PetID, args.Item.IncidentTypeID));

            recyclerView.SetAdapter(_reminderAdapter);
        }

        private void DeleteSelectedItems(ActionMode mode)
        {
            AlertDialogHelper.DisplayDialog(Activity, "Remove selected reminders", "Are you sure? This will remove ALL selected reminders!", () =>
            {
                var dialog = ProgressDialogHelper.Display(Activity, "Removing selected reminders...");

                ProgressDialogHelper.RunTask(Activity, dialog, () =>
                {
                    _reminderAdapter.RemoveSelectedAlarms();
                    Activity.RunOnUiThread(() => mode.Finish());
                });
            });
        }

        public void OnItemCheckedStateChanged(ActionMode mode, int position, long id, bool @checked) { }

        public bool OnActionItemClicked(ActionMode mode, IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Resource.Id.action_delete:
                    DeleteSelectedItems(mode);
                    return true;
                case Resource.Id.action_select_all:
                    _reminderAdapter.SetAllItemsChecked(true);
                    return true;
                default:
                    return false;
            }
        }

        public bool OnCreateActionMode(ActionMode mode, IMenu menu)
        {
            mode.MenuInflater.Inflate(Resource.Menu.menu_list, menu);
            return true;
        }

        public void OnDestroyActionMode(ActionMode mode) { }

        public bool OnPrepareActionMode(ActionMode mode, IMenu menu) => false;
    }
}