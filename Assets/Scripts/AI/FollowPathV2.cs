﻿using UnityEngine;
using System.Collections;

public class FollowPathV2 : MonoBehaviour
{
    public bool followFromBehind = false;

    public Transform follower;
    float previousDistanceToFollower;

    public BezierSpline startingSpline;    
    public BezierSpline currentSpline;
    float t;

    public float segmentLength = 0.003f;
    public float speed = 1f;

    // for dealing with arbitrary oriented splines
    Vector3 lastForward = Vector3.zero;
    public bool reverse = false;

    public float distanceSinceLapStart = 0;

    public float desiredDistToFollower = 50;

    // Use this for initialization
    void Start()
    {
        t = 0;
        currentSpline = startingSpline;

        transform.position = currentSpline.GetPoint(t);
        transform.rotation = currentSpline.GetOrientation(t, Vector3.Lerp(currentSpline.startNode.transform.up, currentSpline.endNode.transform.up, t));

        lastForward = transform.forward;

        //currentSpline.GetComponent<MeshCollider>().enabled = true;

    }

    public float GetCurrentCurvature()
    {
        return currentSpline.GetCurvatureRadius(t).magnitude;
    }

    // This behaviour assumes that the paths has been selected -> All splines has been already marked as path.
    void Update()
    {        
        AdjustSpeed();

        float currentDistance = 0;
        Vector3 lastPoint = transform.position;
        Vector3 newPoint = Vector3.zero;

        if (speed == 0) return;

        while (currentDistance <= speed)
        {

            if (!reverse)
            {
                //t += (speed * Time.deltaTime) / currentSpline.length;
                t += segmentLength;
                if (t > 1) ChangeToNextSpline();

                newPoint = currentSpline.GetPoint(t);
                //transform.rotation = currentSpline.GetOrientation(t, Vector3.Lerp(currentSpline.startNode.transform.up, currentSpline.endNode.transform.up, t));

                if (Vector3.Dot(transform.forward, lastForward) < 0) transform.rotation *= Quaternion.Euler(0, 180, 0);
                lastForward = transform.forward;
            }
            else
            {
                //t -= (speed * Time.deltaTime) / currentSpline.length;
                t -= segmentLength;

                if (t < 0) ChangeToNextSpline();

                newPoint = currentSpline.GetPoint(t);
                //transform.rotation = currentSpline.GetOrientation(t, Vector3.Lerp(currentSpline.startNode.transform.up, currentSpline.endNode.transform.up, t));

                if (Vector3.Dot(transform.forward, lastForward) < 0) transform.rotation *= Quaternion.Euler(0, 180, 0);
                lastForward = transform.forward;

            }

            currentDistance += Vector3.Distance(lastPoint, newPoint);
            lastPoint = newPoint;


            distanceSinceLapStart += segmentLength;

        }

        transform.position = newPoint;
        transform.rotation = currentSpline.GetOrientation(t, Vector3.Lerp(currentSpline.startNode.transform.up, currentSpline.endNode.transform.up, t));

    }

    void AdjustSpeed()
    {
        if (follower != null)
        {
            float distanceToFollower = 0;
            float distanceToObjective = 0;

            if (followFromBehind)
            {
                //speed = follower.transform.GetComponentInChildren<FollowPathV2>().speed;

                distanceToFollower = follower.transform.GetComponentInChildren<FollowPathV2>().distanceSinceLapStart - distanceSinceLapStart;
                distanceToObjective = distanceToFollower - desiredDistToFollower;

                //if (distanceToObjective > 0) speed += 0.05f * distanceToObjective;
                //if (distanceToObjective < 0) speed += 0.1f * distanceToObjective;

                speed = distanceToObjective;

                speed = Mathf.Clamp(speed, 0, float.MaxValue);

                return;
            }
            else
            {
                distanceToFollower = Vector3.Distance(transform.position, follower.position);
                //if (isDeathWall) distanceToFollower *= -1;
                //float distanceToObjective = follower.parent.GetComponent<AICarMovementV3>().distToNode - distanceToFollower;
                distanceToObjective = desiredDistToFollower - distanceToFollower;
            }           

            float derivate = 0;
            if (Time.deltaTime != 0)
            {
                derivate = (distanceToObjective - previousDistanceToFollower) / Time.deltaTime;
                //if (isDeathWall && distanceToObjective < 0) derivate *= -1;
            }

            speed += derivate * Time.deltaTime;

            if (distanceToObjective < 0) speed = 0.001f;

            speed = Mathf.Clamp(speed, 0, float.MaxValue);            

            previousDistanceToFollower = distanceToObjective;

            //if (distanceToObjective < 0 && isDeathWall) speed = 0;
                        
        }
        else
        {
            speed = 0.0001f;
        }
    }

    void ChangeToNextSpline()
    {
        TrackElement currentCurve = currentSpline.curve;

        if (currentSpline.curve.GetType() == typeof(Curve))
        {// If we are in a Curve
            int currentSplineIndex = currentCurve.splines.IndexOf(currentSpline);

            if (!reverse)
            {
                // If there are more splines in the current curve, change to next
                if (currentSplineIndex + 1 <= currentCurve.splines.Count - 1)
                {
                    currentSpline = currentCurve.splines[currentSplineIndex + 1];
                    t = t - 1;
                    return;
                }
                else
                { // Jump to next TrackElement                    

                    if (((Curve)currentCurve).nextCurve.GetType() == typeof(Curve))
                    {// Jump from curve to curve
                        Curve nextCurve = (Curve)((Curve)currentCurve).nextCurve;
                        if (nextCurve.nextCurve == currentCurve)
                        {// --->/<---
                            currentSpline = nextCurve.splines[nextCurve.splines.Count - 1];
                            reverse = true;
                            // Update t
                            t = 1 - (t - 1);
                        }
                        else if (nextCurve.previousCurve == currentCurve)
                        {// --->/--->
                            currentSpline = nextCurve.splines[0];
                            reverse = false;
                            // Update t
                            t = t - 1;
                        }
                    }

                    if (((Curve)currentCurve).nextCurve.GetType() == typeof(Bifurcation))
                    {// Jump from curve to bifurcation.
                        Bifurcation nextBifurcation = (Bifurcation)((Curve)currentCurve).nextCurve;

                        if (nextBifurcation.nextCurveRight == currentCurve)
                        {// --->/---> (From Right path to Start)
                            currentSpline = nextBifurcation.splines[0];
                            reverse = true;
                            // Update t
                            t = 1 - (t - 1);
                        }
                        else if (nextBifurcation.nextCurveLeft == currentCurve)
                        {// --->/---> (From Left path to Start)
                            currentSpline = nextBifurcation.splines[1];
                            reverse = true;
                            // Update t
                            t = 1 - (t - 1);
                        }

                        // Here we are sure that we are entering the bifurcation from the start node.
                        else if (nextBifurcation.splines[0].isPath)
                        {// --->/---> (From Start to Right path)
                            currentSpline = nextBifurcation.splines[0];
                            reverse = false;
                            // Update t
                            t = t - 1;
                        }
                        else if (nextBifurcation.splines[1].isPath)
                        {// --->\---> (From Start to Left path)
                            currentSpline = nextBifurcation.splines[1];
                            reverse = false;
                            // Update t
                            t = t - 1;
                        }
                    }


                }
            }
            else // reverse mode
            {
                // If there are more splines in the current curve, change to previous spline
                if (currentSplineIndex - 1 >= 0)
                {
                    currentSpline = currentCurve.splines[currentSplineIndex - 1];
                    t = 1 + t;
                    return;
                }
                else
                { // Jump to next TrackElement

                    if (((Curve)currentCurve).previousCurve.GetType() == typeof(Curve))
                    {// Jump to curve
                        Curve previousCurve = (Curve)((Curve)currentCurve).previousCurve;
                        if (previousCurve.nextCurve == currentCurve)
                        {// <---/<---
                            currentSpline = previousCurve.splines[((Curve)currentCurve).previousCurve.splines.Count - 1];
                            reverse = true;
                            // Update t
                            t = t + 1;
                        }
                        else if (previousCurve.previousCurve == currentCurve)
                        {// <---/--->
                            currentSpline = previousCurve.splines[0];
                            reverse = false;
                            // Update t
                            t = 1 - (t + 1);
                        }

                    }
                    if (((Curve)currentCurve).previousCurve.GetType() == typeof(Bifurcation))
                    {// Jump from curve to bifurcation.
                        Bifurcation nextBifurcation = (Bifurcation)(((Curve)currentCurve).previousCurve);

                        if (nextBifurcation.nextCurveRight == currentCurve)
                        {// --->/---> (From Right path to Start)
                            currentSpline = nextBifurcation.splines[0];
                            reverse = true;
                            // Update t
                            t = 1 + t;
                        }
                        else if (nextBifurcation.nextCurveLeft == currentCurve)
                        {// --->\---> (From Left path to Start)
                            currentSpline = nextBifurcation.splines[1];
                            reverse = true;
                            // Update t
                            t = 1 + t;
                        }
                        else if (nextBifurcation.previousCurve == currentCurve)
                        {// --->\---> (From Start to..)
                            if (nextBifurcation.splines[0].isPath)
                            {// --->/---> (From Start to Right path)
                                currentSpline = nextBifurcation.splines[0];
                                reverse = false;
                                // Update t
                                t = 1 - (1 - t);
                            }
                            else if (nextBifurcation.splines[1].isPath)
                            {// --->\---> (From Start to Left path)
                                currentSpline = nextBifurcation.splines[1];
                                reverse = false;
                                // Update t
                                t = 1 - (1 - t);
                            }
                        }
                    }

                }
            }
        }
        else if (currentSpline.curve.GetType() == typeof(Bifurcation))
        {// If we are in a Bifurcation
            Bifurcation bifurcation = (Bifurcation)currentSpline.curve;
            if (!reverse)
            {
                // Start to Right
                if (currentSpline == bifurcation.splines[0] && bifurcation.nextCurveRight.previousCurve == bifurcation)
                {// Start to Straight Right
                    if (bifurcation.nextCurveRight.GetType() == typeof(Bifurcation))
                    {// bifurcation to bifurcation
                        Bifurcation nextBifurcation = (Bifurcation)(bifurcation.nextCurveRight);
                        if (nextBifurcation.splines[0].isPath) currentSpline = nextBifurcation.splines[0];
                        else if (nextBifurcation.splines[1].isPath) currentSpline = nextBifurcation.splines[1];

                        reverse = false;
                        // Update t
                        t = t - 1;
                    }
                    else
                    {// bifurcation to Straight curve
                        currentSpline = bifurcation.nextCurveRight.splines[0];
                        reverse = false;
                        // Update t
                        t = t - 1;
                    }
                }
                else if (currentSpline == bifurcation.splines[0])
                {// Start to Reverse Curve
                    currentSpline = bifurcation.nextCurveRight.splines[bifurcation.nextCurveRight.splines.Count - 1];
                    reverse = true;
                    // Update t
                    t = 1 - (1 - t);
                }

                // Start to Left
                if (currentSpline == bifurcation.splines[1] && bifurcation.nextCurveLeft.previousCurve == bifurcation)
                {// Start to Straight Left
                    if (bifurcation.nextCurveLeft.GetType() == typeof(Bifurcation))
                    {// bifurcation to bifurcation
                        Bifurcation nextBifurcation = (Bifurcation)(bifurcation.nextCurveLeft);
                        if (nextBifurcation.splines[0].isPath) currentSpline = nextBifurcation.splines[0];
                        else if (nextBifurcation.splines[1].isPath) currentSpline = nextBifurcation.splines[1];

                        reverse = false;
                        // Update t
                        t = t - 1;
                    }
                    else
                    {// bifurcation to Straight curve
                        currentSpline = bifurcation.nextCurveLeft.splines[0];
                        reverse = false;
                        // Update t
                        t = t - 1;
                    }
                }
                else if (currentSpline == bifurcation.splines[1])
                {// Start to Reverse Curve
                    currentSpline = bifurcation.nextCurveLeft.splines[bifurcation.nextCurveLeft.splines.Count - 1];
                    reverse = true;
                    // Update t
                    t = 1 - (1 - t);
                }
            }
            else
            {// Reverse mode
                if (bifurcation.previousCurve.GetType() == typeof(Bifurcation))
                {// back to another bifurcation
                    Bifurcation previousBif = (Bifurcation)bifurcation.previousCurve;
                    if (previousBif.nextCurveRight == bifurcation) currentSpline = previousBif.splines[0];
                    if (previousBif.nextCurveLeft == bifurcation) currentSpline = previousBif.splines[1];

                    reverse = true;
                    t = t + 1;
                }
                else
                {// back to a Curve
                    if (bifurcation.previousCurve.previousCurve == bifurcation)
                    { // <--/-->
                        currentSpline = bifurcation.previousCurve.splines[0];
                        reverse = false;
                        t = 1 - (1 - t);
                    }
                    else
                    {
                        currentSpline = bifurcation.previousCurve.splines[bifurcation.previousCurve.splines.Count - 1];
                        reverse = true;
                        t = t + 1;
                    }
                }
            }
        }

        //currentSpline.GetComponent<MeshCollider>().enabled = true;
        return;
    }
    
}
