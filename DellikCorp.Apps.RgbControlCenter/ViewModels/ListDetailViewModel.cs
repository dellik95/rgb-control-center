namespace DellikCorp.Apps.RgbControlCenter.ViewModels;

public partial class ListDetailViewModel : BaseViewModel
{
	readonly IArduinoConnector dataService;

	[ObservableProperty]
	bool isRefreshing;

	[ObservableProperty]
	ObservableCollection<ConnectedDevice> items;

    public ListDetailViewModel(IArduinoConnector service)
	{
		dataService = service;
    }

	[RelayCommand]
	private async void OnRefreshing()
	{
		IsRefreshing = true;

		try
		{
			await LoadDataAsync();
		}
		finally
		{
			IsRefreshing = false;
		}
	}

	[RelayCommand]
	public async Task LoadMore()
    {
        var devices = await this.dataService.FindConnectedArduino();

		foreach (var device in devices)
		{
			Items.Add(device);
		}
	}

	public async Task LoadDataAsync()
	{
		Items = new ObservableCollection<ConnectedDevice>(await dataService.FindConnectedArduino());
	}

	[RelayCommand]
	private async void GoToDetails(ConnectedDevice item)
	{
		await Shell.Current.GoToAsync(nameof(ListDetailDetailPage), true, new Dictionary<string, object>
		{
			{ "Device", item }
		});
	}
}
