   M S B A M L   `   `   `  ����O  KDegaussingTestZigApp, Version=1.0.0.0, Culture=neutral, PublicKeyToken=nullS.clr-namespace:DegaussingTestZigApp.Views.Pages DegaussingTestZigApp.Views.Pages  5      4    .DegaussingTestZigApp.Views.Pages.DashboardPage   5      W SPresentationCore, Version=9.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=9.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=9.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089R NWindowsBase, Version=9.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     5      R NSystem.Xaml, Version=9.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5      @d2http://schemas.microsoft.com/expression/blend/2008     5      @local.clr-namespace:DegaussingTestZigApp.Views.Pages     5      Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5      M IWpf.Ui, Version=4.0.0.0, Culture=neutral, PublicKeyToken=11f9f5cc97b3ffd6I EWpf.Ui.Violeta, Version=4.0.3.1, Culture=neutral, PublicKeyToken=null1ui&http://schemas.lepo.co/wpfui/2022/xaml   5        N� Title$  DashboardPage��5	         ApplicationBackgroundBrush  Wpf.Ui.Markup.Design   
Background# �   5        TextFillColorPrimaryBrush   
Foreground# �  5       N� 
Foreground# �  5      $
��True��5      .��5      �  � ColumnDefinitions 5   
   �� 5      $��* �6   .+�5   
   � -   5         �Name$ dashboardGrid��5      $��0��6   $y�Visible=�66    � RowDefinitions 5      �� 5      $
��auto �6    �� 5      $
��auto �6    �� 5      $
��auto �6    .+�5!      � $��0��6   .+�5"        Wpf.Ui.Controls.Card  
���OO/�$��0,0,0,15q�65   .��5#      ��  �� Orientation$ 
Horizontal=�6!   $��0,0,0,0q�6:   .+�5$         Wpf.Ui.Controls.SymbolIcon     Symbol$ Info28=�6(     Wpf.Ui.Controls.FontIcon	   FontSize$	 18(�68   
   
FontWeight$

 Bold"�6F   $��0,0,5,0q�6X     Wpf.Ui.Controls.TextBlock  5%      $
��Left=�6'   $��Center=�6B   $��15(�6]   $
��Bold"�6k   $'��!테스트를 위한 임시 배포��6}   5&      5'      5(      � 5+      $��1��6   $��0,0,0,0q�6    .+�5,      �� .+�5-      �� $!��● 통신 모듈 테스트��6    $��15(�63   $��SemiBold"�6A   $��0,0,0,15q�6W   5.      5/      � 52      $��2��6   $��0,0,0,15q�6     53      �� 54      $��* �6'   �� 55      $��* �6'   .+�59      � $��0��6   $��0,0,15,0q�6'    5:      �� 5;      $
��auto �6(   .+�5>      � $��0��6   .+�5?        .��5@      �� .+�5A   "   �� $ 
Horizontal=�6-   $��Center=�6F   $��0,0,0,10q�6a   .+�5B   &   �� $'��!이더넷 통신 모듈 테스트��60   
�������$��14(�6X   $��SemiBold"�6f   $��0,5,0,0q�6|     Wpf.Ui.Controls.Button  5C   &   -   5        $��통신설정��5C   0   $	��100��6?   $��30��6K   $��20,0,0,0q�6W   5D   0   5E   #   �� 5G   "   $ 
Horizontal=�6-   $��	0,10,0,10q�6F   .+�5H   &   �� $��Connect��6-   $��0,0,10,0q�6u   ��6?   �� *#!ViewModel.ModbusUDPConnectCommand+6u   �� 5I   &   $��
Disconnect��6-   $��0,0,10,0q�6{   ��6B   �� *&$ViewModel.ModbusUDPDisconnectCommand+6{   �� 5J   &   
��� � �$��0,5,10,0q�6u   ��60   �� *" ViewModel.ModbusUDPstatusMessage+6u   5K   #     5M   "   $��0,0,0,0q�6*   .��5N   &     �� 5O   *   $��20(�6\   $
��Bold"�6j   
���\\���6.   �� *ViewModel.UDPResponseList[0]+6|   5P   '   5Q   #     5S   "   .��5T   &     �� 5U   *   $��20(�6\   $
��Bold"�6j   
���\\���6.   �� *ViewModel.UDPResponseList[1]+6|   5V   '   5W   #     5Y   "   .��5Z   &     �� 5[   *   $��20(�6\   $
��Bold"�6j   
���\\���6.   �� *ViewModel.UDPResponseList[2]+6|   5\   '   5]   #     5_   "   .��5`   &     �� 5a   *   $��20(�6\   $
��Bold"�6j   
���\\���6.   �� *ViewModel.UDPResponseList[3]+6|   5b   '   5c   #     5e   "   .��5f   &     �� 5g   *   $��20(�6\   $
��Bold"�6j   
���\\���6.   �� *ViewModel.UDPResponseList[4]+6|   5h   '   5i   #   5j      5k      5l      5m      � 5q      $��1��6   $��0,0,15,0q�6'    5r      �� 5s      $
��auto �6(   .+�5v      � $��0��6   .+�5w        .��5x      �� .+�5y   "   �� $ 
Horizontal=�6-   $��Center=�6F   $��0,0,0,10q�6a   .+�5z   &   �� $'��!시리얼 통신 모듈 테스트��60   
�������$��14(�6X   $��SemiBold"�6f   $��0,5,0,0q�6}     5{   &   -   5        $��통신설정��5{   0   $	��100��6?   $��30��6K   $��20,0,0,0q�6W   5|   0   5}   #   �� 5   "   $ 
Horizontal=�6-   $��	0,10,0,10q�6F   .+�5�   &   �� $��Connect��6-   $��0,0,10,0q�6u   ��6?   �� *#!ViewModel.ModbusRTUConnectCommand+6u   �� 5�   &   $��
Disconnect��6-   $��0,0,10,0q�6{   ��6B   �� *&$ViewModel.ModbusRTUDisconnectCommand+6{   �� 5�   &   
��� � �$��0,5,10,0q�6u   ��60   �� *" ViewModel.ModbusRTUstatusMessage+6u   �� 5�   &   $��Request��6-   $��0,0,10,0q�6u   ��6?   �� *#!ViewModel.ModbusRTURequestCommand+6u   5�   #     5�   "   $��0,0,0,0q�6*   .��5�   &     �� 5�   *   $��20(�6\   $
��Bold"�6j   
���\\���6.   �� *ViewModel.RTUResponseList[0]+6|   5�   '   5�   #     5�   "   .��5�   &     �� 5�   *   $��20(�6\   $
��Bold"�6j   
���\\���6.   �� *ViewModel.RTUResponseList[1]+6|   5�   '   5�   #     5�   "   .��5�   &     �� 5�   *   $��20(�6\   $
��Bold"�6j   
���\\���6.   �� *ViewModel.RTUResponseList[2]+6|   5�   '   5�   #     5�   "   .��5�   &     �� 5�   *   $��20(�6\   $
��Bold"�6j   
���\\���6.   �� *ViewModel.RTUResponseList[3]+6|   5�   '   5�   #     5�   "   .��5�   &     �� 5�   *   $��20(�6\   $
��Bold"�6j   
���\\���6.   �� *ViewModel.RTUResponseList[4]+6|   5�   '   5�   #   5�      5�      5�      5�      5�      5�      � 5�   
   -   5        $ DialogOverlayHost��5�      
������ $y�	Collapsed=�5�       K� ZIndex$	 999��5�      $��Stretch=�5�      $��Stretch=�5�      .+�5�      �� -   5         ��Name$  RootContentDialogPresenter��5�   #   $��Stretch=�5�      $��Stretch=�5�      5�      5�      5�      