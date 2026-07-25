using System;
using System.Windows;
using System.Windows.Input;
using SellFast.App.ViewModels;

namespace SellFast.App.Views
{
    public partial class OnboardingWindow : Window
    {
        public OnboardingWindow(OnboardingViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
            UpdateStepVisibility(viewModel.CurrentStep);
            viewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(OnboardingViewModel.CurrentStep))
                {
                    UpdateStepVisibility(viewModel.CurrentStep);
                }
            };
        }

        private void UpdateStepVisibility(int step)
        {
            Step1Panel.Visibility = step == 1 ? Visibility.Visible : Visibility.Collapsed;
            Step2Panel.Visibility = step == 2 ? Visibility.Visible : Visibility.Collapsed;
            Step3Panel.Visibility = step == 3 ? Visibility.Visible : Visibility.Collapsed;
            Step4Panel.Visibility = step == 4 ? Visibility.Visible : Visibility.Collapsed;

            BtnPrev.Visibility = step > 1 ? Visibility.Visible : Visibility.Collapsed;
            BtnNext.Visibility = step < 4 ? Visibility.Visible : Visibility.Collapsed;
            BtnFinish.Visibility = step == 4 ? Visibility.Visible : Visibility.Collapsed;
        }

        private void BtnPrev_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is OnboardingViewModel vm)
            {
                UpdateStepVisibility(vm.CurrentStep);
            }
        }

        private void BtnNext_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is OnboardingViewModel vm)
            {
                UpdateStepVisibility(vm.CurrentStep);
            }
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }
    }
}
