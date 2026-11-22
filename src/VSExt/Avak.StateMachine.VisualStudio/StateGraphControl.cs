namespace Avak.StateMachine.VisualStudio;

using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Extensibility.UI;

internal sealed class StateGraphControl : RemoteUserControl
{
    internal const string Xaml = """
        <DataTemplate xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                      xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
          <Grid Margin="10">
            <Grid.RowDefinitions>
              <RowDefinition Height="Auto" />
              <RowDefinition Height="Auto" />
              <RowDefinition Height="*" />
            </Grid.RowDefinitions>
            <Grid.ColumnDefinitions>
              <ColumnDefinition Width="*" />
              <ColumnDefinition Width="Auto" />
              <ColumnDefinition Width="Auto" />
            </Grid.ColumnDefinitions>
            <TextBox Grid.Row="0" Grid.Column="0" Margin="0,0,8,0"
                     Text="{Binding XmlPath, UpdateSourceTrigger=PropertyChanged}" />
            <Button Grid.Row="0" Grid.Column="1" Margin="0,0,8,0" Content="Browse..."
                    Command="{Binding BrowseCommand}" />
            <Button Grid.Row="0" Grid.Column="2" Content="Load graph"
                    Command="{Binding LoadCommand}" />
            <TextBlock Grid.Row="1" Grid.ColumnSpan="3" Margin="0,8,0,8"
                       Text="{Binding StatusMessage}" TextWrapping="Wrap" />
            <ScrollViewer Grid.Row="2" Grid.ColumnSpan="3"
                          HorizontalScrollBarVisibility="Auto"
                          VerticalScrollBarVisibility="Auto">
              <Grid Width="{Binding GraphWidth}" Height="{Binding GraphHeight}">
                <ItemsControl ItemsSource="{Binding Transitions}">
                  <ItemsControl.ItemsPanel>
                    <ItemsPanelTemplate>
                      <Canvas />
                    </ItemsPanelTemplate>
                  </ItemsControl.ItemsPanel>
                  <ItemsControl.ItemContainerStyle>
                    <Style TargetType="{x:Type ContentPresenter}">
                      <Setter Property="Canvas.Left" Value="{Binding X}" />
                      <Setter Property="Canvas.Top" Value="{Binding Y}" />
                    </Style>
                  </ItemsControl.ItemContainerStyle>
                  <ItemsControl.ItemTemplate>
                    <DataTemplate>
                      <Border Background="White" Padding="2">
                        <TextBlock Text="{Binding DisplayLabel}" TextWrapping="Wrap" />
                      </Border>
                    </DataTemplate>
                  </ItemsControl.ItemTemplate>
                </ItemsControl>
                <ItemsControl ItemsSource="{Binding Nodes}">
                  <ItemsControl.ItemsPanel>
                    <ItemsPanelTemplate>
                      <Canvas />
                    </ItemsPanelTemplate>
                  </ItemsControl.ItemsPanel>
                  <ItemsControl.ItemContainerStyle>
                    <Style TargetType="{x:Type ContentPresenter}">
                      <Setter Property="Canvas.Left" Value="{Binding X}" />
                      <Setter Property="Canvas.Top" Value="{Binding Y}" />
                      <Setter Property="Width" Value="{Binding Width}" />
                      <Setter Property="Height" Value="{Binding Height}" />
                    </Style>
                  </ItemsControl.ItemContainerStyle>
                  <ItemsControl.ItemTemplate>
                    <DataTemplate>
                      <Border BorderBrush="#FF4A5568" BorderThickness="2"
                              Background="#FFE2E8F0" Padding="8">
                        <TextBlock Text="{Binding DisplayName}" TextAlignment="Center"
                                   VerticalAlignment="Center" TextWrapping="Wrap" />
                      </Border>
                    </DataTemplate>
                  </ItemsControl.ItemTemplate>
                </ItemsControl>
              </Grid>
            </ScrollViewer>
          </Grid>
        </DataTemplate>
        """;

    public StateGraphControl(StateGraphViewModel viewModel)
        : base(viewModel)
    {
    }

    public override Task<string> GetXamlAsync(CancellationToken cancellationToken) =>
        Task.FromResult(Xaml);
}
