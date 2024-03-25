import pandas as pd
import matplotlib.pyplot as plt

# Load the bc 
bc = pd.read_csv('BC Environment_Cumulative Reward.csv')
bc_gail = pd.read_csv('BC_GAIL Environment_Cumulative Reward.csv')

# Smoothing Function
def smoothing(series, weight=0.99):
    span = 2 / (1 - weight) - 1
    return series.ewm(span=span).mean()

# Filter bc for steps up to 3M
bc_gail = bc_gail[bc_gail['Step'] <= 3000000]
bc = bc[bc['Step'] <= 3000000]

# Apply Smoothing 
bc_gail['Smoothed_Value'] = smoothing(bc_gail['Value'], weight=0.99)
bc['Smoothed_Value'] = smoothing(bc['Value'], weight=0.99)


plt.figure(figsize=(15, 8))

# Original values with Slight Transparency
plt.plot(bc_gail['Step'], bc_gail['Value'], color='lightblue', alpha=0.3)
plt.plot(bc['Step'], bc['Value'], color='lightgreen', alpha=0.3)

# Smoothed Values
plt.plot(bc_gail['Step'], bc_gail['Smoothed_Value'], label='Behavioral Cloning', color='blue', linewidth=2)
plt.plot(bc['Step'], bc['Smoothed_Value'], label='Behavioral Cloning + GAIL', color='green', linewidth=2)

# Enhance the plot
plt.xlim(left=0, right=3000000) # Limit x-axis to 3M
plt.xlabel('Step (in millions)', fontsize=24)
plt.ylabel('Cumulative Reward', fontsize=24)
plt.title('Cumulative Reward vs Steps', fontsize=26)
plt.grid(True)

# Increase font size 
plt.xticks(fontsize=20)
plt.yticks(fontsize=20)

# Set the step values on x-axis
million_formatter = lambda x, pos: f'{x / 1e6:.1f}M' if x % 1e6 != 0 else f'{int(x / 1e6)}M'
plt.gca().xaxis.set_major_formatter(plt.FuncFormatter(million_formatter))

# Add legends 
plt.legend(fontsize=20, loc='upper right')

plt.tight_layout()
plt.show()
