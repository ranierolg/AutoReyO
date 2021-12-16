library(ggplot2)
library(grid)
library(reshape2)

comp2 <- matrix(0, nrow=144, ncol=2)
comp2[,1] <- as.vector(templates[[18]])
comp2[,2] <- as.vector(tests[[18]])
sel <- which(comp2[,1] != 0 | comp2[,2] != 0)
comp2 <- comp2[sel,]

melted_data <- melt(t(vis_orient_res_grid(res_grid(gestures.resampled[[90]], 12))))
point_data <- prep_for_graph(gestures.resampled[[90]])
compare_data <- data.frame(
  bar=seq(from=1, to=nrow(comp2), by=1), 
  val=comp2[,1])
compare2_data <- data.frame(
  bar=seq(from=1, to=nrow(comp2), by=1), 
  val=comp2[,2])

main_plot <- ggplot() + 
  geom_tile(data = melted_data, aes(x=Var1, y=Var2, fill=value, color="black")) +
  guides(color=FALSE, fill=guide_legend("Point Density")) + 
  scale_fill_gradient2(low="white", high="green") +
  scale_colour_manual(values = c("grey50")) +
  theme_bw() +
  theme(axis.title.x=element_blank(),
        axis.text.x=element_blank(),
        axis.ticks.x=element_blank(),
        axis.title.y=element_blank(),
        axis.text.y=element_blank(),
        axis.ticks.y=element_blank(),
        panel.grid=element_blank(),
        panel.border=element_blank(),
        legend.key.size=unit(30, unit="points")) +
  geom_point(
    data=point_data,
    aes(Var1, Var2))


bar_plot <- ggplot() +
  geom_bar(data=compare_data, aes(x=bar, y=val, fill="Template", alpha=0.5), stat="identity", width=1) +
  guides(alpha=FALSE) + 
  geom_bar(data=compare2_data, aes(x=bar, y=val, fill="Test", alpha=0.5), stat="identity", width=1) +
  guides(alpha=FALSE) + 
  scale_fill_manual(values = c("#999999", "#009E73")) +
  labs(x="Nonzero Cells", y="Point Density") + 
  scale_x_discrete(
    breaks=seq(from=1, to=nrow(comp2), by=4), 
    limits=seq(from=1, to=nrow(comp2), by=4)) + 
  theme_bw() + 
  theme(
    panel.border=element_blank(),
    legend.title=element_blank(),
    legend.key.size=unit(30, unit="points"))

point_plot <- ggplot() +
  geom_point(mapping=aes(x=actual, y=predicted, color=factor(freq), size=2, alpha=1), data=scores.freq.unique) +
  guides(size=FALSE) +
  guides(alpha=FALSE) +
  guides(colour = guide_legend("Point Frequency", override.aes = list(size=10))) +
  scale_colour_manual(values=c("#999999", "#E69F00", "#56B4E9", "#009E73")) + 
  scale_x_continuous(
    breaks=seq(from=8, to=40, by=2), 
    minor_breaks=seq(from=8 ,to=40, by=1),
    labels=seq(from=8, to=40, by=2)) +
  scale_y_continuous(
    breaks=seq(from=8, to=40, by=2),
    minor_breaks=seq(from=8 ,to=40, by=1),
    labels=seq(from=8, to=40, by=2)) +
  geom_abline(intercept=0, slope=1, show.legend=FALSE) +
  theme_bw() +
  theme(
    legend.key.size=unit(30, unit="points"))



print(main_plot)
print(bar_plot)
print(point_plot)