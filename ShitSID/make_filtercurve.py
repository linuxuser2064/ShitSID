import math
import matplotlib.pyplot as plt

def make_type3_list(baseresistance, offset, steepness, minimumfetresistance, cap=470e-12, dac=0.966):
    def approximate_dac(x):
        value = 0.0
        bit = 1
        weight = 1.0
        dir_ = 2 * dac
        for _ in range(11):
            if x & bit:
                value += weight
            bit <<= 1
            weight *= dir_
        return value / (weight / dac / dac) * (1 << 11)

    result = []
    for x in range(2048):
        kink = approximate_dac(x)
        dynamic = minimumfetresistance + offset / (steepness ** kink)
        resistance = (baseresistance * dynamic) / (baseresistance + dynamic)
        frequency = 1 / (2 * math.pi * cap * resistance)
        result.append((x, frequency))
    return result

# Default parameters
vals = dict(
    baseresistance=1330501.7614243603,
    offset=284686371.68553483,
    steepness=1.0070878631781752,
    minimumfetresistance=18453.69463236424,
)

# Generate the data
table = make_type3_list(**vals)

# Print in Tuple.Create() style
print("{")
for i, (x, freq) in enumerate(table):
    print(f"Tuple.Create({x}, {freq:.4f}),")
print("}")

# Plot the curve
xs = [x for x, _ in table]
ys = [y for _, y in table]

plt.figure(figsize=(8, 5))
plt.plot(xs, ys, label="Cutoff Frequency", color="green")
plt.title("SID Filter Curve (Type 3 Model)")
plt.xlabel("Cutoff Register (0–2047)")
plt.ylabel("Frequency (Hz)")
plt.grid(True, which="both", linestyle="--", linewidth=0.5)
plt.legend()
plt.tight_layout()
plt.show()
