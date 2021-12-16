library(purrr)
library(gplots)
library(RColorBrewer)
library(pracma)
library(reshape2)
library(ggplot2) 
 
GESTURE_LOCATION <- "../savedGestures"
SAMPLING_RESOLUTION <- as.integer(128)
EPS <- 0.5 # Something between [0..1]

parse_template_file <- function(filename) {
    path <- paste(GESTURE_LOCATION, "/", filename, sep="")
    lines <- readLines(path)
    i <- 5
    x <- as.double(c())
    y <- as.double(c())
    sids <- as.integer(c())
    stroke_id <- 1
    while (i <= length(lines)) {
        if (lines[i] == "===NOTES===") {
            break
        } else if (lines[i] == "END STROKE") {
            stroke_id <- stroke_id + 1  
        } else {
            dataline <- strsplit(lines[i], " ")
            x <- append(x, as.double(dataline[[1]][1]))
            y <- append(y, as.double(dataline[[1]][2]))
            sids <- append(sids, stroke_id)
        }
        i = i + 1 
    } 
    data.frame(x=x, y=y, sid=sids)
}

center_scale <- function(raw) {
    # Mins and Maxes
    min_x <- min(raw$x)
    max_x <- max(raw$x)
    min_y <- min(raw$y)
    max_y <- max(raw$y)
    scale <- pmax(max_x - min_x, max_y - min_y)
    
    # Scales
    x_scaled <- sapply(
        raw$x,
        function (xp) {
            (xp - min_x) / scale
        })
    y_scaled <- sapply(
        raw$y,
        function (yp) {
            (yp - min_y) / scale
        })
    
    # Centers
    x_mean <- mean(x_scaled)
    y_mean <- mean(y_scaled)
    
    data.frame(
        x=(x_scaled - x_mean),
        y=(y_scaled - y_mean),
        sid=raw$sid)
}

resample <- function(cs) {
    # Gets the path length of individual strokes
    eudist <- vector(mode="numeric", length=nrow(cs) - 1)
    for (i in 2:nrow(cs)) {
        if (cs$sid[i] == cs$sid[i-1]) {
            eudist[i-1] <- sqrt(
                (cs$y[i] - cs$y[i-1])^2 +
                (cs$x[i] - cs$x[i-1])^2)
        }
    }
    
    path_length <- sum(eudist) 
    
    # Resampling
    new_points <- data.frame(
        x=vector(mode="double", SAMPLING_RESOLUTION),
        y=vector(mode="double", SAMPLING_RESOLUTION),
        sid=vector(mode="integer", SAMPLING_RESOLUTION))
    new_points[1,] <- cs[1,]
    num_points <- 1
    I <- path_length / (SAMPLING_RESOLUTION - 1)
    D <- 0
    for (i in 2:nrow(cs)) {
        if (cs$sid[i] == cs$sid[i-1]) {
            d <- eudist[i-1]
            if (D+d >= I) {
                firstPoint <- cs[(i-1),]
                while (D+d >= I) {
                    # Add Interpolated point
                    t <- min(
                        max((I-D)/d, 0.0),
                        1.0)
                    # (The previous code included a NaN check)
                    if (is.nan(t)) {t <- 0.5}
                    num_points <- num_points + 1
                    new_points[num_points, 1] <- (1.0 - t) * firstPoint$x + t * cs$x[i]
                    new_points[num_points, 2] <- (1.0 - t) * firstPoint$y + t * cs$y[i]
                    new_points[num_points, 3] <- cs$sid[i]
                    # Update partial length
                    d <- D + d - I
                    D <- 0
                    firstPoint <- new_points[num_points,] 
                }
                D <- d
            } else {
                D <- D + d
            }
        }
    }
    if (num_points < 32) {
        new_points[32,] = cs[nrow(cs),] 
    }
    new_points
}

pdollar.GreedyCloudMatch <- function(test, template) {
    n <- nrow(template)
    step <- n^(1 - EPS)
    min_dist <- .Machine$double.xmax
    i <- 1
    while (i <= n) {
        min_dist <- pmin(
            pmin(
                pdollar.CloudDistance(test, template, i),
                pdollar.CloudDistance(test, template, i)),
            min_dist)
        i <- floor(i + step)
    }
    min_dist
}

pdollar.CloudDistance <- function(test, template, start.i) {
    n <- nrow(test)
    matched <- vector(mode="logical", length=n)
    sum_dist <- 0
    i <- start.i
    repeat {
        index <- 1
        min_dist <- .Machine$double.xmax
        for (j in 1:n) {
            dist <- sqrt(
                (test$x[i] - template$x[j])^2 +
                (test$y[i] - template$y[j])^2)
            if (dist < min_dist) {
                min_dist <- dist
                index <- j
            }
        }
        matched[index] <- TRUE
        weight <- 1.0 - (((i - start.i + n) - 1) %% n)  / (1.0 + n)
        sum_dist <- weight * min_dist
        i <- pmax(((i + 1) %% (n + 1)), 1) 
        if (i == start.i) break
    }
    sum_dist
}

Classify <- function(test, templates, match.FUN) {
    reduce(
        .x=names(templates),
        .init=data.frame(
            score=.Machine$double.xmax,
            template="N/A"),
        .f=function(best, template.name) {
            score <- match.FUN(test, templates[[template.name]])
            if (score < best$score) {
                best$score <- score
                best$template <- template.name
            }
            best
        })
}

# gesture is simply the gesture at hand
# margin represents the number of points calculated from each side
point_density <- function(gesture, window) {
    density <- data.frame(
        x=vector(mode="double", nrow(gesture)),
        y=vector(mode="double", nrow(gesture)))
    for (i in (window + 1):(nrow(gesture) - window)) {
        density$x[i] <- reduce(
            .x=gesture$x[(i - window) : (i + window)],
            .init=0,
            .f=function(sum, p) sum + (p - gesture$x[i])^2)
        density$y[i] <- reduce(
            .x=gesture$y[(i - window) : (i + window)],
            .init=0,
            .f=function(sum, p) sum + (p - gesture$y[i])^2)
    }
    density
}


res_grid <- function(gesture, n) {
    xmax <- max(gesture$x)
    xmin <- min(gesture$x)
    ymax <- max(gesture$y)
    ymin <- min(gesture$y)
    out <- matrix(nrow=n, ncol=n)
    xseq <- seq(from=xmin, to=xmax, length.out=n+1)
    yseq <- seq(from=ymin, to=ymax, length.out=n+1)
    #main_min <- pmin(xmin, ymin)
    #main_max <- pmax(xmax, ymax)
    #xseq <- seq(from=main_min, to=main_max, length.out=n+1)
    #yseq <- seq(from=main_min, to=main_max, length.out=n+1)
    for (i in 1:n) {
        for (j in 1:n) {
            out[i,j] <- length(
                gesture[
                    which(
                        gesture$x >= xseq[i] & 
                        gesture$x <= xseq[i + 1] & 
                        gesture$y >= yseq[j] & 
                        gesture$y <= yseq[j + 1]), 
                    1])
        }
    }
    out / sum(out)
    
}

heatmap_compare <- function(test, template) {
    comp <- template - test
    for (i in 1:nrow(comp)) {
        for (j in 1:ncol(comp)) {
            if (comp[i,j] < 0) {
                comp[i,j] <- 0
            }
        }
    }
    sum(comp)
}

embed_displace <- function(test, dr, dc) {
    k <- ncol(test)
    canvass <- matrix(0, ncol=k, nrow=k)
    c.range.r <- seq(
        from=pmax(1+dr, 1),
        to=pmin(k+dr, k),
        by=1)
    c.range.c <- seq(
        from=pmax(1+dc, 1),
        to=pmin(k+dc, k),
        by=1)
    t.range.r <- seq(
        from=pmax(1-dr, 1),
        to=pmin(k-dr, k),
        by=1)
    t.range.c <- seq(
        from=pmax(1-dc, 1),
        to=pmin(k-dc, k),
        by=1)
    for (i in 1:(k - abs(dr))) {
        canvass[c.range.r[i], c.range.c] <- test[t.range.r[i], t.range.c]
    }
    canvass
}

select_best_placement <- function(test, template, max_disp) {
    k <- ncol(test)
    best <- .Machine$double.xmax
    for (i in -max_disp:max_disp) {
        for (j in -max_disp:max_disp) {
            disp <- embed_displace(test, i, j)
            temp <- heatmap_compare(disp, template)
            best <- pmin(temp, best)
        }
    }
    best
}

create_templates <- function(raw_templates, p, res) {
    n <- length(raw_templates)/p
    raw_mat <- t(matrix(raw_templates, nrow=p, ncol=n))
    vec <- vector("list", length=n)
    for (i in 1:n) {
        final <- reduce(
            .x=raw_mat[i,],
            .init=matrix(0,  nrow=res, ncol=res),
            .f=function (sum, mat) {
                sum + res_grid(mat, res)  
            })
       vec[[i]] <- final/sum(final)
    }
    vec
}

get_template_stats <- function(test, templates, disp) {
    scores <- sapply(
        templates,
        function(template){
            select_best_placement(test, template, disp)
        })
    m <- which(scores == min(scores))
    data.frame(
        match=scores[m],
        index=m,
        others.mean=mean(scores[-m]),
        others.sd=sd(scores[-m]))
}

evaluate_templates <- function (tests, templates, disp) {
    n <- length(tests)
    sts <- data.frame(
        match=vector(mode="double", length=n),
        index=vector(mode="integer", length=n),
        others.mean=vector(mode="double", length=n),
        others.sd=vector(mode="double", length=n))
    for (i in 1:n) {
        sts[i, ] <- get_template_stats(tests[[i]], templates, disp)
    }
    sts
}

prep_for_graph <- function(dt) {
    dt$x <- -dt$x
    rot <- -pi
    mat <- t(data.matrix(dt)[,1:2])
    rot.mat <- matrix(c(cos(rot), -sin(rot), sin(rot), cos(rot)), nrow=2, ncol=2)
    final <- rot.mat %*% mat
    
    x_scale <- max(final[1,]) - min(final[1,])
    y_scale <- max(final[2,]) - min(final[2,])
    Var1 <- .5 + ((final[1,] - min(final[1,])) / x_scale) * 12
    Var2 <- .5 + ((final[2,] - min(final[2,])) / y_scale) * 12
    
    
    data.frame(
        Var1=Var1,
        Var2=Var2,
        value=nrow(dt))
}

vis_orient_res_grid <- function(mat) {
    apply(
        mat,
        1,
        function (row) {
            rev(row)
        })
}

#########################
######## PROGRAM ########
#########################

# get the raw data
gestures.raw <- vector("list", length=0)
filenames <- list.files(GESTURE_LOCATION)
for (name in filenames) {
    gestures.raw[[name]] <- parse_template_file(name)
}

# create gesture file - test and template data
gestures.sc <- lapply(
    gestures.raw,
    center_scale)
gestures.resampled <- lapply(
    gestures.sc,
    resample)

# split the dataset
gestures.split <- seq(from=1, to=90, by=5)
gestures.template <- gestures.resampled[-gestures.split]
gestures.test <- gestures.resampled[gestures.split]

# Create and process templates
tests <- create_templates(gestures.test, 1, 12)
templates <- create_templates(gestures.template, 4, 12)
tpp <- evaluate_templates(tests, templates, 6)



#my_map <- heatmap.2(
#    tests[[3]], 
#    dendrogram='none', 
#    Rowv=FALSE, 
#    Colv=FALSE,
#    trace='none', 
#    col=colorRampPalette(c("grey", "green"))(n = 299), 
#    key=FALSE)

#my_map <- heatmap.2(
#    templates[[18]], 
#    dendrogram='none', 
#    Rowv=FALSE, 
#    Colv=FALSE,
#    trace='none', 
#    col=colorRampPalette(c("grey", "green"))(n = 299), 
#    key=FALSE,
#    lhei=c(0,4),
#    lwid=c(0,4))



